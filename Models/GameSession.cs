using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class GameSession
    {
        public int gameSessionId { get; set; }
        public int creatorUserId { get; set; }
        public User creatorUser { get; set; }

        public int opponentUserId { get; set; }
        public User opponentUser { get; set; }


        public DateTime startedAt { get; set; } = DateTime.UtcNow;
        public GameSessionState state { get; set; } = GameSessionState.PENDING;
        public int currentTurnIndex { get; set; } = 0;
        public int? winnerUserId { get; set; } = null;
        public int currentTurnPlayerId { get; set; }

        public List<SessionCharacterState> participants { get; set; } = new();
        public List<GameActionLog> actionLog { get; set; } = new();

        // Position boundaries (e.g., linear board from 0 to 100)
        public float minPosition { get; set; } = 0f;
        public float maxPosition { get; set; } = 100f;
    }
}
