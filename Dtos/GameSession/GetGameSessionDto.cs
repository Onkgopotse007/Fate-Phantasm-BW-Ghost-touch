using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.GameSession
{
    public class GetGameSessionDto
    {
        public int gameSessionId { get; set; }

        public int creatorUserId { get; set; }
        public string creatorUsername { get; set; }

        public int opponentUserId { get; set; }
        public string opponentUsername { get; set; }

        public DateTime startedAt { get; set; }
        public GameSessionState state { get; set; }
        public int currentTurnIndex { get; set; }
        public int currentTurnPlayerId { get; set; }
        public int? winnerUserId { get; set; }

        public float minPosition { get; set; }
        public float maxPosition { get; set; }

        public List<GetSessionCharacterStateDto> participants { get; set; } = new();
    }
}
