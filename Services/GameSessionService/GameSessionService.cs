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
            var response = new ServiceResponse<GetGameSessionDto>();

            if (newSessionDto.characterIds == null || newSessionDto.characterIds.Count != 2)
            {
                throw new GenericException("Exactly two character IDs must be provided to create a session.", 422);
            }

            var characters = await _context.Characters
                .Where(c => newSessionDto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
            {
                throw new GenericException("One or more character IDs are invalid.", 422);
            }

            bool hasSupport = characters.Any(c => c.role == RoleType.Support);
            bool hasVanguard = characters.Any(c => c.role == RoleType.Vanguard);

            if (!hasSupport || !hasVanguard)
            {
                throw new GenericException("A valid team must consist of one Support and one Vanguard character.", 422);
            }

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

            response.data = _mapper.Map<GetGameSessionDto>(session);
            response.success = true;
            return response;
        }
        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetActiveGameSessionsAsync(int userId)
        {
            var response = new ServiceResponse<List<GetGameSessionDto>>();

            var sessions = await _context.GameSessions
                .Where(gs => gs.state == GameSessionState.ACTIVE &&
                    gs.participants.Any(p => p.userId == userId))
                .Include(gs => gs.creatorUser)
                .Include(gs => gs.opponentUser)
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.user)
                .ToListAsync();

            response.data = sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList();
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> GetGameSessionByIdAsync(int sessionId)
        {
            var response = new ServiceResponse<GetGameSessionDto>();

            var session = await _context.GameSessions
                .Include(gs => gs.creatorUser)
                .Include(gs => gs.opponentUser)
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId) ?? throw new NotFoundException("Session not found.");
            response.data = _mapper.Map<GetGameSessionDto>(session);
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> MoveCharacterAsync(int userId, MoveActionDto dto)
        {
            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE) ?? throw new NotFoundException("Active session not found.");
            Functions.EnsureUserTurn(session, userId);

            var character = session.participants.FirstOrDefault(p => p.characterId == dto.characterId && p.userId == userId) ?? throw new NotFoundException("Character not found in session or not owned by user.");
            if (!character.isAlive)
                throw new GenericException("Character is not alive.", 400);

            if (session.currentTurnPlayerId != userId)
                throw new GenericException("Player has already acted this turn.", 400);

            if (character.role != RoleType.Vanguard)
                throw new GenericException("Only Vanguard characters are allowed to move.", 403);

            int movement = character.character.movement;
            int currentPos = (int)character.xPosition;
            int newPos;

            if (character.team == TeamSide.CREATOR)
            {
                newPos = (int)Math.Min(currentPos + movement, session.maxPosition);
            }
            else if (character.team == TeamSide.OPPONENT)
            {
                newPos = (int)Math.Max(currentPos - movement, session.minPosition);
            }
            else
            {
                throw new GenericException("Invalid team type for character.", 422);
            }

            character.xPosition = newPos;
            character.hasActedThisTurn = true;

            var nextPlayer = session.participants.FirstOrDefault(p => p.userId != session.currentTurnPlayerId)
                             ?? throw new GenericException("Could not complete turn", 422);

            session.currentTurnPlayerId = nextPlayer.userId;

            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AttackCharacterAsync(int userId, AttackCharacterDto dto)
        {
            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.state == GameSessionState.ACTIVE) ?? throw new NotFoundException("Active session not found.");
            Functions.EnsureUserTurn(session, userId);

            var attacker = session.participants.FirstOrDefault(p => p.characterId == dto.attackerId && p.userId == userId);
            var target = session.participants.FirstOrDefault(p => p.characterId == dto.targetId);

            if (attacker == null || target == null)
                throw new NotFoundException("Invalid attacker or target.");

            if (!attacker.isAlive || !target.isAlive)
                throw new GenericException("One or both characters are not alive.", 400);

            if (attacker.character.role == RoleType.Vanguard && !Functions.IsWithinProximity(attacker.xPosition, target.xPosition))
                throw new GenericException("Vanguard characters can only attack targets in close proximity.", 400);


            if (attacker.hasActedThisTurn)
                throw new GenericException("Attacker has already acted this turn.", 400);

            target.currentHealth -= attacker.character.baseDamage;
            if (target.currentHealth <= 0)
            {
                target.isAlive = false;
                target.currentHealth = 0;
            }

            if (attacker.currentMana < attacker.character.mana)
            {
                attacker.currentMana = Math.Min(
                    attacker.currentMana + attacker.character.manaGainPerAttack,
                    attacker.character.mana
                );
            }

            attacker.hasActedThisTurn = true;

            var nextPlayer = session.participants.FirstOrDefault(p => p.userId != session.currentTurnPlayerId)
                             ?? throw new GenericException("Could not complete turn", 422);

            session.currentTurnPlayerId = nextPlayer.userId;

            Functions.CheckVictoryCondition(session);
            Functions.AdvanceTurn(session);

            await _context.SaveChangesAsync();

            return new ServiceResponse<GetGameSessionDto>
            {
                success = true,
                data = _mapper.Map<GetGameSessionDto>(session)
            };
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AbandonSessionAsync(int userId, int sessionId)
        {
            var response = new ServiceResponse<GetGameSessionDto>();

            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId && gs.state != GameSessionState.ABANDONED) ?? throw new NotFoundException("Session not found.");
            session.state = GameSessionState.ABANDONED;

            await _context.SaveChangesAsync();

            response.data = _mapper.Map<GetGameSessionDto>(session);
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AcceptSessionAsync(int userId, AcceptGameSessionDto dto)
        {
            var response = new ServiceResponse<GetGameSessionDto>();

            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId && gs.opponentUserId == userId) ?? throw new NotFoundException("Session not found.");
            if (session.state != GameSessionState.PENDING)
            {
                throw new NotFoundException("Session is not in a pending state.");
            }

            if (dto.characterIds == null || dto.characterIds.Count != 2)
            {
                throw new GenericException("Exactly two character IDs must be provided to create a session.", 422);
            }

            var characters = await _context.Characters
                .Where(c => dto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
            {
                throw new GenericException("One or more character IDs are invalid.", 422);
            }

            bool hasSupport = characters.Any(c => c.role == RoleType.Support);
            bool hasVanguard = characters.Any(c => c.role == RoleType.Vanguard);

            if (!hasSupport || !hasVanguard)
            {
                throw new GenericException("A valid team must consist of one Support and one Vanguard character.", 422);
            }

            var random = new Random();
            var startingPlayer = session.participants[random.Next(session.participants.Count)];
            session.currentTurnPlayerId = startingPlayer.userId;

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
                    team = TeamSide.OPPONENT
                });
            }

            session.state = GameSessionState.ACTIVE;
            await _context.SaveChangesAsync();

            response.data = _mapper.Map<GetGameSessionDto>(session);
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> RejectSessionAsync(int userId, int sessionId)
        {
            var response = new ServiceResponse<GetGameSessionDto>();

            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId) ?? throw new NotFoundException("Session not found.");
            if (session.state != GameSessionState.PENDING)
            {
                throw new GenericException("Only pending sessions can be rejected.", 422);
            }

            session.state = GameSessionState.REJECTED;
            await _context.SaveChangesAsync();

            response.data = _mapper.Map<GetGameSessionDto>(session);
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> CastSpellAsync(int userId, CastSpellDto dto)
        {
            var response = new ServiceResponse<GetGameSessionDto>();

            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                        .ThenInclude(c => c.abilities)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId) ?? throw new NotFoundException("Game session not found.");
            Functions.EnsureUserTurn(session, dto.userId);

            var caster = session.participants
                .FirstOrDefault(p => p.characterId == dto.casterId && p.userId == dto.userId);
            var target = session.participants
                .FirstOrDefault(p => p.characterId == dto.targetId);

            if (caster == null || !caster.isAlive)
            {
                throw new GenericException("Caster not found or is not alive.", 422);
            }

            if (target == null || !target.isAlive)
            {
                throw new GenericException("Target not found or is not alive.", 422);
            }

            if (caster.character.role == RoleType.Vanguard && !Functions.IsWithinProximity(caster.xPosition, target.xPosition))
                throw new GenericException("Vanguard characters can only cast spells on nearby targets.", 422);


            if (session.currentTurnPlayerId != userId)
            {
                throw new GenericException("Caster has already acted this turn.", 422);
            }

            var ability = caster.character.abilities
                .FirstOrDefault(a => a.id == dto.abilityId) ?? throw new GenericException("Ability not found.", 422);
            if (caster.currentMana < ability.manaCost)
            {
                throw new GenericException("Not enough mana to cast the spell.", 422);
            }

            // Deduct mana and apply damage
            caster.currentMana -= ability.manaCost;
            target.currentHealth -= ability.damage;

            if (target.currentHealth <= 0)
            {
                target.currentHealth = 0;
                target.isAlive = false;
            }

            caster.hasActedThisTurn = true;

            // Change turn and check win conditions
            var nextPlayer = session.participants
                .FirstOrDefault(p => p.userId != session.currentTurnPlayerId)
                ?? throw new GenericException("Could not complete turn", 422);

            session.currentTurnPlayerId = nextPlayer.userId;

            Functions.CheckVictoryCondition(session);
            Functions.AdvanceTurn(session);

            await _context.SaveChangesAsync();

            response.success = true;
            response.data = _mapper.Map<GetGameSessionDto>(session);
            return response;
        }
        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetGameSessionsByUserIdAsync(int userId, GameSessionState? state = null)
        {

            var response = new ServiceResponse<List<GetGameSessionDto>>();
            var query = _context.GameSessions
                .Include(gs => gs.creatorUser)
                .Include(gs => gs.opponentUser)
                .Include(gs => gs.participants)
                    .ThenInclude(p => p.character)
                .Where(gs => gs.creatorUserId == userId || gs.opponentUserId == userId);

            if (state.HasValue)
                query = query.Where(gs => gs.state == state.Value);

            var sessions = await query.ToListAsync();

            if (!sessions.Any())
                throw new NotFoundException("No game sessions found for the specified user");

            response.data = sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList();
            response.success = true;
            return response;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> JoinGameSessionAsync(JoinGameSessionDto dto)
        {
            var response = new ServiceResponse<GetGameSessionDto>();
            var session = await _context.GameSessions
                .Include(gs => gs.participants)
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId) ?? throw new NotFoundException("Game session not found");
            if (session.opponentUserId != dto.opponentUserId)
                throw new GenericException("You are not authorized to join this session", 403);

            if (session.state != GameSessionState.PENDING)
                throw new GenericException("This session is not joinable", 400);

            if (session.participants.Any(p => p.userId == dto.opponentUserId))
                throw new GenericException("You have already joined this session", 400);

            var characters = await _context.Characters
                .Where(c => dto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != dto.characterIds.Count)
                throw new GenericException("One or more characters are invalid or do not belong to the user", 400);

            foreach (var character in characters)
            {
                session.participants.Add(new SessionCharacterState
                {
                    sessionId = session.gameSessionId,
                    characterId = character.id,
                    userId = dto.opponentUserId,
                    character = character,
                    currentHealth = character.hitpoints,
                    currentMana = 0.1 * character.mana,
                    maxHealth = character.hitpoints,
                    maxMana = character.mana,
                    isAlive = true,
                    role = character.role,
                    hasActedThisTurn = false,
                    xPosition = session.maxPosition,
                    team = TeamSide.OPPONENT
                });
            }

            session.state = GameSessionState.ACTIVE;
            var random = new Random();
            session.currentTurnPlayerId = random.Next(0, 2) == 0
                ? session.creatorUserId
                : session.opponentUserId;

            session.currentTurnIndex = 0;

            await _context.SaveChangesAsync();

            response.data = new GetGameSessionDto
            {
                gameSessionId = session.gameSessionId,
                startedAt = session.startedAt,
                state = session.state,
                currentTurnIndex = session.currentTurnIndex,
                currentTurnPlayerId = session.currentTurnPlayerId,
                winnerUserId = session.winnerUserId,
                minPosition = session.minPosition,
                maxPosition = session.maxPosition,
                participants = session.participants.Select(p => new GetSessionCharacterStateDto
                {
                    sessionCharacterId = p.sessionCharacterId,
                    sessionId = p.sessionId,
                    characterId = p.characterId,
                    characterName = p.character?.name ?? "Unknown",
                    userId = p.userId,
                    currentHealth = p.currentHealth,
                    currentMana = p.currentMana,
                    maxHealth = p.maxHealth,
                    maxMana = p.maxMana,
                    isAlive = p.isAlive,
                    role = p.role,
                    hasActedThisTurn = p.hasActedThisTurn,
                    xPosition = p.xPosition,
                    team = p.team
                }).ToList()
            };

            return response;
        }

    }
}