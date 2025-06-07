using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.GameSession
{
    public class JoinGameSessionDto
    {
        public int sessionId { get; set; }
        public int opponentUserId { get; set; }
        public List<int> characterIds { get; set; }
    }
}