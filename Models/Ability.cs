using System.Collections.Generic;

namespace RPG_dotnet.Models
{
    public class Ability
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int manaCost { get; set; } = 0;
        public ICollection<CharacterAbility> characterAbilities { get; set; }
    }
}