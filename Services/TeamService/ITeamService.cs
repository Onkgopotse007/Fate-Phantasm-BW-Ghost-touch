using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Services.TeamService
{
    public interface ITeamService
    {
        Task<ServiceResponse<List<GetCharacterDto>>> AssignCharacterToUser(int userId,int characterId);
        Task<ServiceResponse<List<GetCharacterDto>>> GetUserCharacters(int userId);
        Task<ServiceResponse<List<GetCharacterDto>>> GetUserCharacter(int userId, int characterId);
    }
}