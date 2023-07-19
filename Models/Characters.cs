using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class Characters
    {
        public int id { get; set; }
        public string name { get; set; } = "Shirou Emiya";
        public int hitpoints { get; set; } = 10;
        public int strength { get; set; } = 10;
        public int defense { get; set; } = 10;
        public int intelligence { get; set; } = 10;
        public RpgClass fighterClass {get; set;} = RpgClass.Archer;
        public ICollection<UserCharacter> userCharacters { get; set; }
        
    }
}