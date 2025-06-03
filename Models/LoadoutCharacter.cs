using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class LoadoutCharacter
    {
        public int loadoutId { get; set; }
        public Loadout loadout { get; set; }

        public int characterId { get; set; }
        public Characters character { get; set; }
    }
}