using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class AddAbilityDto
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int manaCost { get; set; }
        public int damage { get; set; }
    }
}
