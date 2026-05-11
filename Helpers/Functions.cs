using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Helpers
{
    public class Functions
    {
        public async Task validateDtoAsync<TDto>(TDto dto, IValidator<TDto> validator)
        {
            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors;
                throw new FluentValidation.ValidationException("Input validation failed", validationErrors);
            }
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public static void AdvanceTurn(GameSession session)
        {
            var playerIds = session.participants
                .Select(p => p.userId)
                .Distinct()
                .ToList();

            if (playerIds.Count < 2) return;

            int nextPlayerId = playerIds.First(id => id != session.currentTurnPlayerId);

            // Reset the incoming player's characters for their turn
            foreach (var p in session.participants.Where(p => p.userId == nextPlayerId && p.isAlive))
                p.hasActedThisTurn = false;

            // If all alive characters have now acted, it's a new full round
            bool fullRoundComplete = session.participants
                .Where(p => p.isAlive)
                .All(p => p.hasActedThisTurn);

            if (fullRoundComplete)
            {
                session.currentTurnIndex++;
                foreach (var p in session.participants.Where(p => p.isAlive))
                    p.hasActedThisTurn = false;
            }

            session.currentTurnPlayerId = nextPlayerId;
        }

        public static void CheckVictoryCondition(GameSession session)
        {
            var alivePlayerIds = session.participants
                .Where(p => p.isAlive)
                .Select(p => p.userId)
                .Distinct()
                .ToList();

            if (alivePlayerIds.Count <= 1)
            {
                session.state = GameSessionState.COMPLETED;
                session.winnerUserId = alivePlayerIds.Count == 1 ? alivePlayerIds[0] : null;
            }
        }

        public static void EnsureUserTurn(GameSession session, int userId)
        {
            if (session.currentTurnPlayerId != userId)
                throw new GenericException("It is not your turn.", 422);
        }

        public static bool IsWithinProximity(float attackerX, float targetX, float allowedDistance = 10f)
        {
            return Math.Abs(attackerX - targetX) <= allowedDistance;
        }

    }
}