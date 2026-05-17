using System.Collections.Generic;

namespace RPG_dotnet.Models
{
    public class Ability
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int manaCost { get; set; } = 0;
        public int damage { get; set; } = 0;
        public int characterId { get; set; }
        public Characters character { get; set; }
        public AbilityEffect effect { get; set; } = AbilityEffect.None;
        public int effectValue { get; set; } = 0;

        // Who this ability can legally target
        public AbilityTargetType targetType { get; set; } = AbilityTargetType.Enemy;

    }
}