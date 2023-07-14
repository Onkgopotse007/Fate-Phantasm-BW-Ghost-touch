using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class AddCharacterDto
    {
        public required string name { get; set; }
        public int hitpoints { get; set; } 
        public int strength { get; set; }
        public int defense { get; set; }
        public int intelligence { get; set; }
        public RpgClass fighterClass {get; set;}
    }
}