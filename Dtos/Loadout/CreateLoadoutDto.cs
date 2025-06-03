using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Loadout
{
    public class CreateLoadoutDto
    {
        public string name { get; set; }
        public List<int> characterIds { get; set; }
    }
}