using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class SessionCharacterState
    {
        [Key]
        public int sessionCharacterId { get; set; }
        public int sessionId { get; set; }
        public GameSession session { get; set; }

        public int characterId { get; set; }
        public Characters character { get; set; }

        public int userId { get; set; }
        public User user { get; set; }

        public float xPosition { get; set; } = 0f;

        public int currentHealth { get; set; }
        public double currentMana { get; set; }

        public int maxHealth { get; set; }
        public int maxMana { get; set; }

        public bool isAlive { get; set; } = true;

        public RoleType role { get; set; }
        public bool hasActedThisTurn { get; set; } = false;
        public TeamSide team { get; set; }
    }
}