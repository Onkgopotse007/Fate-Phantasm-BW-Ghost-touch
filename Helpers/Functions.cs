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
            // Reset action flags if all alive participants have acted
            if (session.participants.Where(p => p.isAlive).All(p => p.hasActedThisTurn))
            {
                foreach (var participant in session.participants.Where(p => p.isAlive))
                {
                    participant.hasActedThisTurn = false;
                }
            }

            // Move to next alive participant
            int total = session.participants.Count;
            for (int i = 1; i <= total; i++)
            {
                int nextIndex = (session.currentTurnIndex + i) % total;
                if (session.participants[nextIndex].isAlive)
                {
                    session.currentTurnIndex = nextIndex;
                    break;
                }
            }
        }

        public static void CheckVictoryCondition(GameSession session)
        {
            var aliveUserIds = session.participants
                .Where(p => p.isAlive)
                .Select(p => p.userId)
                .Distinct()
                .ToList();

            if (aliveUserIds.Count <= 1)
            {
                session.state = GameSessionState.COMPLETED;
            }
        }

        public static void EnsureUserTurn(GameSession session, int userId)
        {
            var currentPlayer = session.participants.FirstOrDefault(p => p.user.id == session.currentTurnPlayerId) ?? throw new GenericException("Current turn player not found in session participants.", 422);
            if (userId != currentPlayer.user.id)
            {
                throw new GenericException("It is not your turn.", 422);
            }
        }
        public static bool IsWithinProximity(float attackerX, float targetX, float allowedDistance = 10f)
        {
            return Math.Abs(attackerX - targetX) <= allowedDistance;
        }

    }
}