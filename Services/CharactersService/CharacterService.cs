using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPG_dotnet.Helpers;

namespace RPG_dotnet.Services.CharactersService
{
    public class CharacterService : ICharacterService
    {
        private static List<Characters> characters = new List<Characters>{
            new Characters(),
            new Characters{id=2 ,name="King Arthur", fighterClass= RpgClass.Saber}
        };
        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character  = _mapper.Map<Characters>(newCharacter);
            character.id = characters.Max(c => c.id)+1;
            characters.Add(_mapper.Map<Characters>(newCharacter));
            serviceResponse.data = characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacters(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = characters.FirstOrDefault(c=> c.id == id);
            if(character is null)
                throw new NotFoundException($"Character with id '{id}' not found.");
            characters.Remove(character);
            serviceResponse.data = characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = characters.FirstOrDefault(c => c.id == id);
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return  serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            serviceResponse.data = characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = characters.FirstOrDefault(c=> c.id == updateCharacter.id);
            if(character is null)
                throw new NotFoundException($"Character with id '{updateCharacter.id}' not found.");
            character.name = updateCharacter.name;
            character.defense = updateCharacter.defense;
            character.fighterClass = updateCharacter.fighterClass;
            character.hitpoints = updateCharacter.hitpoints;
            character.intelligence = updateCharacter.intelligence;
            character.strength = updateCharacter.strength;
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return  serviceResponse;
        }

    
    }
}