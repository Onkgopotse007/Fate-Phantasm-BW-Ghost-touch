using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RPG_dotnet;
using RPG_dotnet.Data;
using RPG_dotnet.Dtos.GameSession;
using RPG_dotnet.Hubs;
using RPG_dotnet.Models;
using RPG_dotnet.Services.GameSessionService;
using Xunit;

namespace RPG_dotnet.Tests.Services;

// Covers the auto-skip behaviour of the turn engine: when the incoming
// player has no living character able to act (typically because every
// survivor was stunned), the server should end that dead turn itself
// rather than stranding the player, while never looping forever.
public class GameSessionServiceTurnEngineTests
{
    private const int UserA = 1;
    private const int UserB = 2;

    private readonly DataContext _context;
    private readonly IGameSessionService _service;

    public GameSessionServiceTurnEngineTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            // Unique name per instance so parallel test classes never share state.
            .UseInMemoryDatabase(databaseName: $"TurnEngineTests_{System.Guid.NewGuid()}")
            .Options;
        _context = new DataContext(options);
        _context.Database.EnsureCreated();

        var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AutoMapperProfile()))
            .CreateMapper();

        // The hub is only used to broadcast updates; stub it so awaits complete.
        var clientProxy = new Mock<IGameSessionClient>();
        clientProxy.Setup(c => c.SessionUpdated(It.IsAny<GetGameSessionDto>()))
            .Returns(Task.CompletedTask);
        clientProxy.Setup(c => c.SessionCompleted(It.IsAny<GetGameSessionDto>()))
            .Returns(Task.CompletedTask);

        var hubClients = new Mock<IHubClients<IGameSessionClient>>();
        hubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);

        var hub = new Mock<IHubContext<GameSessionHub, IGameSessionClient>>();
        hub.Setup(h => h.Clients).Returns(hubClients.Object);

        _service = new GameSessionService(_context, mapper, hub.Object);
    }

    // Builds an ACTIVE two-player session where it is UserA's turn. Each side
    // gets a single character; the callers tweak the resulting participant
    // states (stun, poison, etc.) before acting.
    private GameSession SeedSession(
        System.Action<SessionCharacterState>? configureA = null,
        System.Action<SessionCharacterState>? configureB = null)
    {
        _context.Users.AddRange(
            new User { id = UserA, userName = "playerA" },
            new User { id = UserB, userName = "playerB" });

        var charA = new Characters { id = 1, name = "CharA", isPlayable = true };
        var charB = new Characters { id = 2, name = "CharB", isPlayable = true };
        _context.Characters.AddRange(charA, charB);

        var pA = new SessionCharacterState
        {
            characterId = charA.id,
            userId = UserA,
            currentHealth = 100,
            maxHealth = 100,
            currentMana = 10,
            maxMana = 20,
            isAlive = true,
            role = RoleType.Vanguard,
            team = TeamSide.CREATOR,
            hasActedThisTurn = false
        };
        var pB = new SessionCharacterState
        {
            characterId = charB.id,
            userId = UserB,
            currentHealth = 100,
            maxHealth = 100,
            currentMana = 10,
            maxMana = 20,
            isAlive = true,
            role = RoleType.Vanguard,
            team = TeamSide.OPPONENT,
            hasActedThisTurn = false
        };

        configureA?.Invoke(pA);
        configureB?.Invoke(pB);

        var session = new GameSession
        {
            creatorUserId = UserA,
            opponentUserId = UserB,
            currentTurnPlayerId = UserA,
            state = GameSessionState.ACTIVE,
            currentTurnIndex = 0,
            participants = { pA, pB }
        };

        _context.GameSessions.Add(session);
        _context.SaveChanges();
        return session;
    }

    [Fact]
    public async Task EndTurn_WhenOnlySurvivorIsStunned_AutoSkipsBackToActingPlayer()
    {
        var session = SeedSession(configureB: b => b.isStunned = true);

        await _service.EndTurnAsync(UserA, new EndTurnDto { sessionId = session.gameSessionId });

        var reloaded = await _context.GameSessions
            .Include(gs => gs.participants)
            .Include(gs => gs.actionLog)
            .FirstAsync(gs => gs.gameSessionId == session.gameSessionId);

        // B's dead turn was auto-skipped, so control is back with A.
        Assert.Equal(UserA, reloaded.currentTurnPlayerId);

        // The stun was consumed at B's (skipped) turn boundary.
        var charB = reloaded.participants.Single(p => p.userId == UserB);
        Assert.False(charB.isStunned);

        // The skipped turn was recorded against B's character for traceability.
        Assert.Contains(reloaded.actionLog,
            log => log.actionType == GameActionType.EndTurn && log.actorCharacterId == 2);
    }

    [Fact]
    public async Task EndTurn_WhenBothSidesAreStunned_DoesNotLoopForever()
    {
        var session = SeedSession(
            configureA: a => a.isStunned = true,
            configureB: b => b.isStunned = true);

        // If the skip loop were unbounded this call would never return.
        await _service.EndTurnAsync(UserA, new EndTurnDto { sessionId = session.gameSessionId });

        var reloaded = await _context.GameSessions
            .Include(gs => gs.participants)
            .Include(gs => gs.actionLog)
            .FirstAsync(gs => gs.gameSessionId == session.gameSessionId);

        // The game is still in progress and settled on a real player.
        Assert.Equal(GameSessionState.ACTIVE, reloaded.state);
        Assert.Contains(reloaded.currentTurnPlayerId, new[] { UserA, UserB });

        // Both stuns were consumed rather than lingering.
        Assert.All(reloaded.participants, p => Assert.False(p.isStunned));

        // Auto-skips are bounded: one manual EndTurn plus at most one skip per
        // player. Guards against a silent runaway loop that still terminates.
        var endTurns = reloaded.actionLog.Count(log => log.actionType == GameActionType.EndTurn);
        Assert.True(endTurns <= 3, $"Expected a bounded number of EndTurn logs, got {endTurns}.");
    }

    [Fact]
    public async Task EndTurn_WhenSkippedTurnCharacterIsPoisoned_PoisonStillTicks()
    {
        var session = SeedSession(configureB: b =>
        {
            b.isStunned = true;
            b.poisonStacks = 2;
            b.poisonDamagePerTick = 5;
            b.currentHealth = 100;
        });

        await _service.EndTurnAsync(UserA, new EndTurnDto { sessionId = session.gameSessionId });

        var charB = await _context.SessionCharacterStates
            .FirstAsync(p => p.userId == UserB);

        // Poison applied exactly once as B's turn started, even though the turn
        // was auto-skipped due to the stun.
        Assert.Equal(95, charB.currentHealth);
        Assert.Equal(1, charB.poisonStacks);
        Assert.False(charB.isStunned);
    }
}
