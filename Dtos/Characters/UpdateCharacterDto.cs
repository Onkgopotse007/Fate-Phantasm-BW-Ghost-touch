using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class UpdateCharacterDto
    {
        public int id { get; set; }
        public int hitpoints { get; set; }
        public int strength { get; set; }
        public int defense { get; set; }
        public int intelligence { get; set; }
        public int mana { get; set; }
        public int movement { get; set; }
        public int baseDamage { get; set; }
        public int manaGainPerAttack { get; set; }
        public List<AddAbilityDto> abilities { get; set; } // instead of abilityIds

    }
}