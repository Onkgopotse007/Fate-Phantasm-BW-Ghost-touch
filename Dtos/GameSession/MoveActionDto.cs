using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.GameSession
{
    public class MoveActionDto
    {
        public int sessionId { get; set; }
        public int characterId { get; set; }
        public MoveDirection direction { get; set; } = MoveDirection.Forward;
    }
}