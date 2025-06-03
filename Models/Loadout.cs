using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class Loadout
    {
        public int loadoutId { get; set; }
        public int userId { get; set; }
        public User user { get; set; }
        public string name { get; set; }

        public List<LoadoutCharacter> characters { get; set; } = new();
    }
}