using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.Team
{
    public class CreateTeamDto
    {
        public int userId { get; set; }
        public int characterId { get; set; }
    }
}