using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Characters
{
    public class GetAbilityDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string effect { get; set; }
        public int manaCost { get; set; }

        public int damage { get; set; }
    }

}