using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Services.CharactersService
{
    public interface ICharacterService
    {
        Task<ServiceResponse<List<Characters>>> GetCharacters();
        Task<ServiceResponse<Characters>> GetCharacterById(int id);
        Task<ServiceResponse<List<Characters>>> AddCharacter(Characters newCharacter);
    }
}