using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG_dotnet.Dtos.GameSession;
using RPG_dotnet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Services.GameSessionService
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public GameSessionService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> CreateGameSessionAsync(int userId, CreateGameSessionDto newSessionDto)
        {
            if (newSessionDto.characterIds == null || newSessionDto.characterIds.Count != 2)
                throw new GenericException("Exactly two character IDs must be provided.", 422);

            var characters = await _context.Characters
                .Where(c => newSessionDto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
                throw new GenericException("One or more character IDs are invalid.", 422);

            ValidateTeamComposition(characters);

            var session = new GameSession
            {
                creatorUserId = userId,
                opponentUserId = newSessionDto.opponentUserId,
                currentTurnPlayerId = userId,
                startedAt = DateTime.UtcNow,
                state = GameSessionState.PENDING,
                currentTurnIndex = 0,
                minPosition = 0f,
                maxPosition = 100f,
                participants = new List<SessionCharacterState>()
            };

            foreach (var character in characters)
            {
                session.participants.Add(new SessionCharacterState
                {
                    session = session,
                    characterId = character.id,
                    userId = userId,
                    xPosition = session.minPosition,
                    currentHealth = character.hitpoints,
                    maxHealth = character.hitpoints,
                    currentMana = 0.1 * character.mana,
                    maxMana = character.mana,
                    isAlive = true,
                    role = character.role,
                    hasActedThisTurn = false,
                    team = TeamSide.CREATOR
                });
            }

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Created new session successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }
        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetActiveGameSessionsAsync(int userId)
        {
            var sessions = await SessionWithFullIncludes()
                .Where(gs => gs.state == GameSessionState.ACTIVE &&
                             gs.participants.Any(p => p.userId == userId))
                .ToListAsync();

            return new ServiceResponse<List<GetGameSessionDto>>
            {
                message = "Retrieved active sessions successfully",
                success = true,
                data = sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList()
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> GetGameSessionByIdAsync(int sessionId)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId)
                          ?? throw new NotFoundException("Session not found.");

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Retrieved session successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> MoveCharacterAsync(int userId, MoveActionDto dto)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE)
                          ?? throw new NotFoundException("Active session not found.");

            Functions.EnsureUserTurn(session, userId);

            var character = session.participants
                                .FirstOrDefault(p => p.characterId == dto.characterId && p.userId == userId)
                            ?? throw new NotFoundException("Character not found in session or not owned by user.");

            if (!character.isAlive)
                throw new GenericException("Character is not alive.", 400);

            if (character.role != RoleType.Vanguard)
                throw new GenericException("Only Vanguard characters can move.", 403);

            if (character.hasActedThisTurn)
                throw new GenericException("This character has already acted this turn.", 400);

            float newPos = character.team == TeamSide.CREATOR
                ? Math.Min(character.xPosition + character.character.movement, session.maxPosition)
                : Math.Max(character.xPosition - character.character.movement, session.minPosition);

            character.xPosition = newPos;
            character.hasActedThisTurn = true;

            LogAction(session, GameActionType.Move, character.characterId, newPosition: newPos);

            TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();

            return ServiceResponse<GetGameSessionDto>.Success(_mapper.Map<GetGameSessionDto>(session), "Character moved successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AttackCharacterAsync(int userId, AttackCharacterDto dto)
        {
            var session = await SessionWithFullIncludes()
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE)
                ?? throw new NotFoundException("Active session not found.");

            Functions.EnsureUserTurn(session, userId);

            var attacker = session.participants
                .FirstOrDefault(p => p.characterId == dto.attackerId && p.userId == userId)
                ?? throw new NotFoundException("Attacker not found in session or not owned by user.");

            var target = session.participants
                .FirstOrDefault(p => p.characterId == dto.targetId)
                ?? throw new NotFoundException("Target not found in session.");

            if (!attacker.isAlive || !target.isAlive)
                throw new GenericException("One or both characters are not alive.", 400);

            if (attacker.role != RoleType.Vanguard)
                throw new GenericException("Only Vanguard characters can perform a basic attack.", 403);

            if (!Functions.IsWithinProximity(attacker.xPosition, target.xPosition))
                throw new GenericException("Vanguard can only attack targets in close proximity.", 400);

            if (attacker.hasActedThisTurn)
                throw new GenericException("This character has already acted this turn.", 400);

            if (target.userId == userId)
                throw new GenericException("Cannot attack your own character.", 400);

            int damage = attacker.character.baseDamage;
            target.currentHealth = Math.Max(0, target.currentHealth - damage);
            if (target.currentHealth <= 0) target.isAlive = false;

            double manaGained = 0;
            if (attacker.currentMana < attacker.maxMana)
            {
                double before = attacker.currentMana;
                attacker.currentMana = Math.Min(
                    attacker.currentMana + attacker.character.manaGainPerAttack,
                    attacker.maxMana);
                manaGained = attacker.currentMana - before;
            }

            attacker.hasActedThisTurn = true;

            LogAction(session, GameActionType.Attack,
                actorCharacterId: attacker.characterId,
                targetCharacterId: target.characterId,
                damageDealt: damage,
                manaGained: manaGained);

            Functions.CheckVictoryCondition(session);

            if (session.state == GameSessionState.ACTIVE)
                TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();

            return ServiceResponse<GetGameSessionDto>.Success(_mapper.Map<GetGameSessionDto>(session), "Character attacked successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AbandonSessionAsync(int userId, int sessionId)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId &&
                                                         gs.state != GameSessionState.ABANDONED)
                          ?? throw new NotFoundException("Session not found.");

            if (session.creatorUserId != userId && session.opponentUserId != userId)
                throw new GenericException("You are not a participant in this session.", 403);

            session.state = GameSessionState.ABANDONED;
            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Session abandoned successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AcceptSessionAsync(int userId, AcceptGameSessionDto dto)
        {
            var session = await SessionWithFullIncludes()
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.opponentUserId == userId)
                ?? throw new NotFoundException("Session not found.");

            if (session.state != GameSessionState.PENDING)
                throw new GenericException("Only pending sessions can be accepted.", 422);

            if (dto.characterIds == null || dto.characterIds.Count != 2)
                throw new GenericException("Exactly two character IDs must be provided.", 422);

            var characters = await _context.Characters
                .Where(c => dto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
                throw new GenericException("One or more character IDs are invalid.", 422);

            ValidateTeamComposition(characters);

            foreach (var character in characters)
            {
                session.participants.Add(new SessionCharacterState
                {
                    session = session,
                    characterId = character.id,
                    userId = userId,
                    xPosition = session.maxPosition,
                    currentHealth = character.hitpoints,
                    maxHealth = character.hitpoints,
                    currentMana = 0.1 * character.mana,
                    maxMana = character.mana,
                    isAlive = true,
                    role = character.role,
                    hasActedThisTurn = false,
                    team = TeamSide.OPPONENT
                });
            }

            var random = new Random();
            session.currentTurnPlayerId = random.Next(2) == 0
                ? session.creatorUserId
                : session.opponentUserId;
            session.currentTurnIndex = 0;
            session.state = GameSessionState.ACTIVE;

            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Session accepted successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> RejectSessionAsync(int userId, int sessionId)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId && gs.opponentUserId == userId)
                          ?? throw new NotFoundException("Session not found.");

            if (session.state != GameSessionState.PENDING)
                throw new GenericException("Only pending sessions can be rejected.", 422);

            session.state = GameSessionState.REJECTED;
            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Session rejected successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> CastSpellAsync(int userId, CastSpellDto dto)
        {
            var session = await SessionWithFullIncludes()
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE)
                ?? throw new NotFoundException("Game session not found.");

            Functions.EnsureUserTurn(session, userId);

            var caster = session.participants
                .FirstOrDefault(p => p.characterId == dto.casterId && p.userId == userId)
                ?? throw new GenericException("Caster not found or not owned by this user.", 422);

            var target = session.participants
                .FirstOrDefault(p => p.characterId == dto.targetId)
                ?? throw new GenericException("Target not found.", 422);

            if (!caster.isAlive)
                throw new GenericException("Caster is not alive.", 422);

            if (!target.isAlive)
                throw new GenericException("Target is not alive.", 422);

            if (caster.hasActedThisTurn)
                throw new GenericException("This character has already acted this turn.", 400);

            if (caster.role == RoleType.Vanguard &&
                !Functions.IsWithinProximity(caster.xPosition, target.xPosition))
                throw new GenericException("Vanguard characters can only cast spells on nearby targets.", 422);

            var ability = caster.character.abilities
                .FirstOrDefault(a => a.id == dto.abilityId)
                ?? throw new GenericException("Ability not found on this character.", 422);

            if (caster.currentMana < ability.manaCost)
                throw new GenericException("Not enough mana to cast this spell.", 422);

            caster.currentMana -= ability.manaCost;
            int damage = ability.damage;
            target.currentHealth = Math.Max(0, target.currentHealth - damage);
            if (target.currentHealth <= 0) target.isAlive = false;

            caster.hasActedThisTurn = true;

            LogAction(session, GameActionType.CastSpell,
                actorCharacterId: caster.characterId,
                targetCharacterId: target.characterId,
                abilityId: ability.id,
                damageDealt: damage,
                manaSpent: ability.manaCost);

            Functions.CheckVictoryCondition(session);

            if (session.state == GameSessionState.ACTIVE)
                TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Spell cast successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }
        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetGameSessionsByUserIdAsync(int userId, GameSessionState? state = null)
        {
            var query = SessionWithFullIncludes()
                .Where(gs => gs.creatorUserId == userId || gs.opponentUserId == userId);

            if (state.HasValue)
                query = query.Where(gs => gs.state == state.Value);

            var sessions = await query.ToListAsync();

            if (!sessions.Any())
                throw new NotFoundException("No game sessions found for the specified user.");

            return new ServiceResponse<List<GetGameSessionDto>>
            {
                message = "Retrieved game sessions successfully",
                success = true,
                data = sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList()
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> EndTurnAsync(
            int userId, EndTurnDto dto)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE)
                          ?? throw new NotFoundException("Active session not found.");

            Functions.EnsureUserTurn(session, userId);

            foreach (var p in session.participants.Where(p => p.userId == userId && p.isAlive))
                p.hasActedThisTurn = true;

            LogAction(session, GameActionType.EndTurn,
                actorCharacterId: session.participants.First(p => p.userId == userId).characterId);

            Functions.AdvanceTurn(session);

            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                message = "Turn ended successfully",
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        private IQueryable<GameSession> SessionWithFullIncludes() =>
            _context.GameSessions
                .Include(gs => gs.creatorUser)
                .Include(gs => gs.opponentUser)
                .Include(gs => gs.participants)
                .ThenInclude(p => p.character)
                .ThenInclude(c => c.abilities)
                .Include(gs => gs.participants)
                .ThenInclude(p => p.user)
                .Include(gs => gs.actionLog);

        private static void LogAction(
            GameSession session,
            GameActionType actionType,
            int actorCharacterId,
            int? targetCharacterId = null,
            int? abilityId = null,
            int damageDealt = 0,
            double manaSpent = 0,
            double manaGained = 0,
            float? newPosition = null)
        {
            session.actionLog.Add(new GameActionLog
            {
                sessionId = session.gameSessionId,
                actorCharacterId = actorCharacterId,
                targetCharacterId = targetCharacterId,
                actionType = actionType,
                abilityId = abilityId,
                damageDealt = damageDealt,
                manaSpent = manaSpent,
                manaGained = manaGained,
                newPosition = newPosition,
                turnIndex = session.currentTurnIndex,
                timestamp = DateTime.UtcNow
            });
        }

        private static void TryAdvanceTurn(GameSession session, int userId)
        {
            bool allActed = session.participants
                .Where(p => p.userId == userId && p.isAlive)
                .All(p => p.hasActedThisTurn);

            if (allActed)
                Functions.AdvanceTurn(session);
        }

        private static void ValidateTeamComposition(List<Characters> characters)
        {
            bool hasSupport = characters.Any(c => c.role == RoleType.Support);
            bool hasVanguard = characters.Any(c => c.role == RoleType.Vanguard);

            if (!hasSupport || !hasVanguard)
                throw new GenericException(
                    "A valid team must consist of one Support and one Vanguard character.", 422);
        }

    }
}