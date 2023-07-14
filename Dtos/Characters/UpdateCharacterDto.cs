using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class UpdateCharacterDto
    {
        public int id { get; set; } = 1;
        public int hitpoints { get; set; } = 10;
        public int strength { get; set; } = 10;
        public int defense { get; set; } = 10;
        public int intelligence { get; set; } = 10;
    }
}