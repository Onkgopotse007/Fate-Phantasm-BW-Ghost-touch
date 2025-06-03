using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Loadout
{
    public class GetLoadoutDto
    {
        public int loadoutId { get; set; }
        public string name { get; set; }
        public List<GetCharacterDto> characters { get; set; }
    }
}