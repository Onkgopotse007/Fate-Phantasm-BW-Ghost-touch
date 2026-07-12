using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG_dotnet.Dtos.GameSession;
using RPG_dotnet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RPG_dotnet.Hubs;

namespace RPG_dotnet.Services.GameSessionService
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHubContext<GameSessionHub, IGameSessionClient> _hub;

        public GameSessionService(DataContext context, IMapper mapper, IHubContext<GameSessionHub, IGameSessionClient> hub)
        {
            _context = context;
            _mapper = mapper;
            _hub = hub;
        }

        public async Task<ServiceResponse<GetGameSessionDto>> CreateGameSessionAsync(
            int userId, CreateGameSessionDto newSessionDto)
        {
            if (newSessionDto.characterIds == null || newSessionDto.characterIds.Count != 2)
                throw new GenericException("Exactly two character IDs must be provided.", 422);

            var characters = await LoadAndValidateCharactersAsync(newSessionDto.characterIds);

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
                    team = TeamSide.CREATOR,
                    reviveCount = character.fighterClass == RpgClass.Berserker ? 1 : 0
                });
            }

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();

            var created = await SessionWithFullIncludes()
                .FirstAsync(gs => gs.gameSessionId == session.gameSessionId);

            await PushSessionUpdate(created);

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(created), "Created new session successfully");
        }

        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetActiveGameSessionsAsync(int userId)
        {
            var sessions = await SessionWithFullIncludes()
                .Where(gs => gs.state == GameSessionState.ACTIVE &&
                             gs.participants.Any(p => p.userId == userId))
                .ToListAsync();

            return ServiceResponse<List<GetGameSessionDto>>.Success(
                sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList(),
                "Retrieved active sessions successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> GetGameSessionByIdAsync(int sessionId)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == sessionId)
                          ?? throw new NotFoundException("Session not found.");

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(session), "Retrieved session successfully");
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

            if (character.isStunned)
                throw new GenericException("This character is stunned and cannot act.", 400);

            float newPos;

            if (dto.direction == MoveDirection.Forward)
            {
                newPos = character.team == TeamSide.CREATOR
                    ? Math.Min(character.xPosition + character.character.movement, session.maxPosition)
                    : Math.Max(character.xPosition - character.character.movement, session.minPosition);
            }
            else
            {
                newPos = character.team == TeamSide.CREATOR
                    ? Math.Max(character.xPosition - character.character.movement, session.minPosition)
                    : Math.Min(character.xPosition + character.character.movement, session.maxPosition);
            }

            character.xPosition = newPos;
            character.hasActedThisTurn = true;

            LogAction(session, GameActionType.Move, character.characterId, newPosition: newPos);
            TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(session), "Character moved successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> AttackCharacterAsync(
            int userId, AttackCharacterDto dto)
        {
            var session = await SessionWithFullIncludes()
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId
                                        && gs.state == GameSessionState.ACTIVE)
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

            if (attacker.isStunned)
                throw new GenericException("This character is stunned and cannot act.", 400);

            if (attacker.hasActedThisTurn)
                throw new GenericException("This character has already acted this turn.", 400);

            if (target.userId == userId)
                throw new GenericException("Cannot attack your own character.", 400);

            if (attacker.role == RoleType.Vanguard &&
                !Functions.IsWithinProximity(attacker.xPosition, target.xPosition))
                throw new GenericException("Vanguard characters can only attack targets in close proximity.", 400);

            int damageDealt = ApplyDamage(target, attacker.character.baseDamage);

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
                damageDealt: damageDealt,
                manaGained: manaGained);

            Functions.CheckVictoryCondition(session);

            if (session.state == GameSessionState.ACTIVE)
                TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(session), "Attack successful");
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
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(_mapper.Map<GetGameSessionDto>(session), "Session abandoned successfully");
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

            var characters = await LoadAndValidateCharactersAsync(dto.characterIds);

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
                    team = TeamSide.OPPONENT,
                    reviveCount = character.fighterClass == RpgClass.Berserker ? 1 : 0
                });
            }

            var random = new Random();
            session.currentTurnPlayerId = random.Next(2) == 0
                ? session.creatorUserId
                : session.opponentUserId;
            session.currentTurnIndex = 0;
            session.state = GameSessionState.ACTIVE;

            await _context.SaveChangesAsync();
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(_mapper.Map<GetGameSessionDto>(session), "Session accepted successfully");
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
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(_mapper.Map<GetGameSessionDto>(session), "Session rejected successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> CastSpellAsync(
            int userId, CastSpellDto dto)
        {
            var session = await SessionWithFullIncludes()
                .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId
                                        && gs.state == GameSessionState.ACTIVE)
                ?? throw new NotFoundException("Game session not found.");

            Functions.EnsureUserTurn(session, userId);

            var caster = session.participants
                .FirstOrDefault(p => p.characterId == dto.casterId && p.userId == userId)
                ?? throw new GenericException("Caster not found or not owned by this user.", 422);

            if (!caster.isAlive)
                throw new GenericException("Caster is not alive.", 422);

            if (caster.hasActedThisTurn)
                throw new GenericException("This character has already acted this turn.", 400);

            if (caster.isStunned)
                throw new GenericException("This character is stunned and cannot act.", 400);

            var ability = caster.character.abilities
                .FirstOrDefault(a => a.id == dto.abilityId)
                ?? throw new GenericException("Ability not found on this character.", 422);

            if (caster.currentMana < ability.manaCost)
                throw new GenericException("Not enough mana to cast this spell.", 422);

            // Validate target ownership matches ability target type
            SessionCharacterState target = null;

            if (ability.targetType == AbilityTargetType.Ally)
            {
                // Heal — must target own team
                target = session.participants
                    .FirstOrDefault(p => p.characterId == dto.targetId && p.userId == userId)
                    ?? throw new GenericException("Heal target must be one of your own characters.", 422);
            }
            else if (ability.targetType == AbilityTargetType.Enemy
                     || ability.targetType == AbilityTargetType.AllEnemies)
            {
                // Enemy or MultiTarget — dto.targetId is validated for single
                // target; MultiTarget fetches all enemies in the switch below
                target = session.participants
                    .FirstOrDefault(p => p.characterId == dto.targetId && p.userId != userId)
                    ?? throw new GenericException("Target not found or is not an enemy.", 422);

                if (!target.isAlive)
                    throw new GenericException("Target is not alive.", 422);

                // Proximity check: Vanguard casters must be in range UNLESS
                // the ability is MultiTarget — NP range is not restricted
                if (caster.role == RoleType.Vanguard
                    && ability.effect != AbilityEffect.MultiTarget
                    && !Functions.IsWithinProximity(caster.xPosition, target.xPosition))
                    throw new GenericException(
                        "Vanguard characters can only cast spells on nearby targets.", 422);
            }

            caster.currentMana -= ability.manaCost;
            caster.hasActedThisTurn = true;

            int damageDealt = 0;
            float? pushedToPosition = null;

            switch (ability.effect)
            {
                case AbilityEffect.None:
                    damageDealt = ApplyDamage(target, ability.damage);
                    break;

                case AbilityEffect.DefensePierce:
                    // Gáe Bolg, Tsubame Gaeshi — bypass defense entirely
                    damageDealt = ApplyDamage(target, ability.damage, pierceDefense: true);
                    break;

                case AbilityEffect.Heal:
                    // Garden of Avalon — restore HP to ally, capped at maxHealth
                    int healAmount = Math.Min(ability.effectValue, target.maxHealth - target.currentHealth);
                    target.currentHealth += healAmount;
                    // Log heal amount as negative damage for clarity on frontend
                    damageDealt = -healAmount;
                    break;

                case AbilityEffect.ManaDrain:
                    // Gáe Dearg, Rule Breaker — damage then strip mana
                    damageDealt = ApplyDamage(target, ability.damage);
                    if (target.isAlive)
                        target.currentMana = Math.Max(0, target.currentMana - ability.effectValue);
                    break;

                case AbilityEffect.Stun:
                    // Zabaniya — damage then flag target to skip next action
                    damageDealt = ApplyDamage(target, ability.damage);
                    if (target.isAlive)
                        target.isStunned = true;
                    break;

                case AbilityEffect.Poison:
                    // All the World's Evil — partial damage now, DoT for 2 turns
                    damageDealt = ApplyDamage(target, ability.damage);
                    if (target.isAlive)
                    {
                        target.poisonStacks = 2;
                        target.poisonDamagePerTick = ability.effectValue;
                    }
                    break;

                case AbilityEffect.MultiTarget:
                    // Unlimited Blade Works, Phoebus Catastrophe, Ionioi Hetairoi
                    // Hit all living enemies regardless of position
                    var enemies = session.participants
                        .Where(p => p.userId != userId && p.isAlive)
                        .ToList();
                    foreach (var enemy in enemies)
                        damageDealt += ApplyDamage(enemy, ability.damage);
                    break;

                case AbilityEffect.PositionPush:
                    // Bellerophon — damage then push target away
                    damageDealt = ApplyDamage(target, ability.damage);
                    if (target.isAlive)
                    {
                        target.xPosition = target.team == TeamSide.CREATOR
                            ? Math.Max(target.xPosition - ability.effectValue, session.minPosition)
                            : Math.Min(target.xPosition + ability.effectValue, session.maxPosition);
                        pushedToPosition = target.xPosition;
                    }
                    break;
            }

            LogAction(session, GameActionType.CastSpell,
                actorCharacterId: caster.characterId,
                targetCharacterId: ability.effect == AbilityEffect.MultiTarget
                    ? null
                    : target?.characterId,
                abilityId: ability.id,
                damageDealt: damageDealt,
                manaSpent: ability.manaCost,
                newPosition: pushedToPosition);

            Functions.CheckVictoryCondition(session);

            if (session.state == GameSessionState.ACTIVE)
                TryAdvanceTurn(session, userId);

            await _context.SaveChangesAsync();
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(session), "Spell cast successfully");
        }

        public async Task<ServiceResponse<List<GetGameSessionDto>>> GetGameSessionsByUserIdAsync(
            int userId, GameSessionState? state = null)
        {
            var query = SessionWithFullIncludes()
                .Where(gs => gs.creatorUserId == userId || gs.opponentUserId == userId);

            if (state.HasValue)
                query = query.Where(gs => gs.state == state.Value);

            var sessions = await query.ToListAsync();

            if (!sessions.Any())
                throw new NotFoundException("No game sessions found for the specified user.");

            return ServiceResponse<List<GetGameSessionDto>>.Success(
                sessions.Select(gs => _mapper.Map<GetGameSessionDto>(gs)).ToList(),
                "Retrieved game sessions successfully");
        }

        public async Task<ServiceResponse<GetGameSessionDto>> EndTurnAsync(int userId, EndTurnDto dto)
        {
            var session = await SessionWithFullIncludes()
                              .FirstOrDefaultAsync(gs => gs.gameSessionId == dto.sessionId
                                                         && gs.state == GameSessionState.ACTIVE)
                          ?? throw new NotFoundException("Active session not found.");

            Functions.EnsureUserTurn(session, userId);

            foreach (var p in session.participants.Where(p => p.userId == userId && p.isAlive))
                p.hasActedThisTurn = true;

            LogAction(session, GameActionType.EndTurn,
                actorCharacterId: session.participants.First(p => p.userId == userId).characterId);

            AdvanceTurnAndProcess(session);

            await _context.SaveChangesAsync();
            await PushSessionUpdate(session);

            return ServiceResponse<GetGameSessionDto>.Success(
                _mapper.Map<GetGameSessionDto>(session), "Turn ended successfully");
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

        private static int ApplyDamage(
            SessionCharacterState target,
            int rawDamage,
            bool pierceDefense = false)
        {
            int effectiveDamage = pierceDefense
                ? rawDamage
                : Math.Max(1, rawDamage - target.character.defense / 2);

            target.currentHealth = Math.Max(0, target.currentHealth - effectiveDamage);

            if (target.currentHealth <= 0)
            {
                // Heracles passive: reviveCount > 0 means God Hand intercepts death
                if (target.reviveCount > 0)
                {
                    target.currentHealth = 30;
                    target.reviveCount--;
                }
                else
                {
                    target.isAlive = false;
                }
            }

            return effectiveDamage;
        }

        private static void ProcessTurnStartEffects(GameSession session)
        {
            var incoming = session.participants
                .Where(p => p.userId == session.currentTurnPlayerId && p.isAlive)
                .ToList();

            foreach (var p in incoming)
            {
                if (p.isStunned)
                {
                    p.hasActedThisTurn = true; // forces the character to skip
                    p.isStunned = false;
                }

                if (p.poisonStacks > 0)
                {
                    p.currentHealth = Math.Max(0, p.currentHealth - p.poisonDamagePerTick);
                    p.poisonStacks--;

                    if (p.currentHealth <= 0)
                    {
                        if (p.reviveCount > 0)
                        {
                            p.currentHealth = 30;
                            p.reviveCount--;
                        }
                        else
                        {
                            p.isAlive = false;
                        }
                    }

                    if (p.poisonStacks == 0)
                        p.poisonDamagePerTick = 0;
                }
            }
        }

        // Only advances the turn once all of the current player's alive
        // characters have acted. The actual switch, status-effect
        // processing and dead-turn skipping is delegated to
        // AdvanceTurnAndProcess.
        private static void TryAdvanceTurn(GameSession session, int userId)
        {
            bool allActed = session.participants
                .Where(p => p.userId == userId && p.isAlive)
                .All(p => p.hasActedThisTurn);

            if (allActed)
                AdvanceTurnAndProcess(session);
        }

        // Switches to the next player, applies their start-of-turn status
        // effects (poison ticks, stun expiry) and checks for victory in case
        // poison killed someone on the turn boundary.
        //
        // If the incoming player is left with no living character able to act
        // — e.g. every survivor was stunned this turn — the server would
        // otherwise hand them a dead turn it never ends, forcing a manual
        // endturn call. Instead we log an automatic EndTurn for traceability
        // and advance again.
        //
        // The skip loop is bounded two ways so that a state where BOTH players
        // are fully incapacitated cannot spin forever: we stop as soon as
        // control returns to the player we started from, and never iterate
        // more than once per distinct player regardless.
        private static void AdvanceTurnAndProcess(GameSession session)
        {
            int startingPlayerId = session.currentTurnPlayerId;
            int playerCount = session.participants
                .Select(p => p.userId)
                .Distinct()
                .Count();

            Functions.AdvanceTurn(session);
            ProcessTurnStartEffects(session);
            Functions.CheckVictoryCondition(session);

            for (int skips = 0;
                 skips < playerCount && session.state == GameSessionState.ACTIVE;
                 skips++)
            {
                // A full lap back to whoever we started from without finding an
                // able player: stop and let them resolve it manually rather
                // than risk looping.
                if (session.currentTurnPlayerId == startingPlayerId)
                    break;

                var incoming = session.participants
                    .Where(p => p.userId == session.currentTurnPlayerId && p.isAlive)
                    .ToList();

                // At least one living character can still act — hand over a
                // real turn.
                if (incoming.Any(p => !p.hasActedThisTurn))
                    break;

                // No living character can act: auto-end this turn (when there is
                // someone to attribute the log to) and advance to the next player.
                if (incoming.Count > 0)
                    LogAction(session, GameActionType.EndTurn,
                        actorCharacterId: incoming.First().characterId);

                Functions.AdvanceTurn(session);
                ProcessTurnStartEffects(session);
                Functions.CheckVictoryCondition(session);
            }
        }

        private static void ValidateTeamComposition(List<Characters> characters)
        {
            bool hasSupport = characters.Any(c => c.role == RoleType.Support);
            bool hasVanguard = characters.Any(c => c.role == RoleType.Vanguard);

            if (!hasSupport || !hasVanguard)
                throw new GenericException(
                    "A valid team must consist of one Support and one Vanguard character.", 422);
        }

        // Shared character validation used by both Create and Accept.
        // Checks IDs exist AND that all selected characters are available
        // for play — isPlayable = false means the character is in the DB
        // but locked (balancing, unreleased, etc).
        private async Task<List<Characters>> LoadAndValidateCharactersAsync(List<int> characterIds)
        {
            var characters = await _context.Characters
                .Where(c => characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != characterIds.Count)
                throw new GenericException("One or more character IDs are invalid.", 422);

            if (characters.Any(c => !c.isPlayable))
                throw new GenericException(
                    "One or more selected characters are not currently available for play.", 422);

            ValidateTeamComposition(characters);

            return characters;
        }

        private async Task PushSessionUpdate(GameSession session)
        {
            var dto = _mapper.Map<GetGameSessionDto>(session);
            var group = GameSessionHub.GroupName(session.gameSessionId);

            if (session.state == GameSessionState.COMPLETED)
                await _hub.Clients.Group(group).SessionCompleted(dto);
            else
                await _hub.Clients.Group(group).SessionUpdated(dto);
        }

    }
}