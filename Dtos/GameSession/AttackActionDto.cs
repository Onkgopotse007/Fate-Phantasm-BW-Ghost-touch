using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.GameSession
{
    public class AttackCharacterDto
    {
        public int sessionId { get; set; }
        public int attackerId { get; set; }
        public int targetId { get; set; }
    }
}