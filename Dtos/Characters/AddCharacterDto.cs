using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class AddCharacterDto
    {
        public string name { get; set; } = "Shirou Emiya";
        public int hitpoints { get; set; } = 10;
        public int strength { get; set; } = 10;
        public int defense { get; set; } = 10;
        public int intelligence { get; set; } = 10;
        public RpgClass fighterClass {get; set;} = RpgClass.Archer;
    }
}