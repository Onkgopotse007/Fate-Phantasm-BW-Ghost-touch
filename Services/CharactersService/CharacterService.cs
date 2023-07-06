using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Services.CharactersService
{
    public class CharacterService : ICharacterService
    {
        private static List<Characters> characters = new List<Characters>{
            new Characters(),
            new Characters{id=1 ,name="King Arthur", fighterClass= RpgClass.Saber}
        };
        public async Task<ServiceResponse<List<Characters>>> AddCharacter(Characters newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<Characters>>();
            characters.Add(newCharacter);
            serviceResponse.data = characters;
            return  serviceResponse;
        }

        public async Task<ServiceResponse<Characters>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<Characters>();
            var character = characters.FirstOrDefault(c => c.id == id);
            serviceResponse.data = character;
            return  serviceResponse;
        }

        public async Task<ServiceResponse<List<Characters>>> GetCharacters()
        {
            var serviceResponse = new ServiceResponse<List<Characters>>();
            serviceResponse.data = characters;
            return  serviceResponse;
        }
    }
}