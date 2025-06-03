using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Loadout
{
    public class UpdateLoadoutDto
    {
        public string name { get; set; }
        public int loadoutId { get; set; }
        public List<int> characterIds { get; set; }
    }

}