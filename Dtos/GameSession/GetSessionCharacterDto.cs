using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.GameSession
{
    public class GetSessionCharacterStateDto
    {
        public int sessionCharacterId { get; set; }
        public int sessionId { get; set; }
        public int characterId { get; set; }
        public string characterName { get; set; }
        public int userId { get; set; }
        public int currentHealth { get; set; }
        public double currentMana { get; set; }
        public int maxHealth { get; set; }
        public int maxMana { get; set; }
        public bool isAlive { get; set; }
        public RoleType role { get; set; }
        public bool hasActedThisTurn { get; set; }
        public float xPosition { get; set; }
        public TeamSide team { get; set; }
        public bool isStunned { get; set; }
        public int poisonStacks { get; set; }
        public int poisonDamagePerTick { get; set; }
        public int reviveCount { get; set; }
    }
}