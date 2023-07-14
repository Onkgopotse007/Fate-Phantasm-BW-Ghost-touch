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
            //new Characters{name="King Arthur", fighterClass= RpgClass.Saber}
        };
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public CharacterService(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character  = _mapper.Map<Characters>(newCharacter);
            _context.Characters.Add(_mapper.Map<Characters>(newCharacter));
            await _context.SaveChangesAsync();
            serviceResponse.data = await _context.Characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacters(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _context.Characters.FirstOrDefault(c=> c.id == id);
            if(character is null)
                throw new NotFoundException($"Character with id '{id}' not found.");
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();
            serviceResponse.data = await _context.Characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.id == id);
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return  serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var characters = await _context.Characters.ToListAsync();
            serviceResponse.data = characters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();
            return  serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = await _context.Characters.FirstOrDefaultAsync(c=> c.id == updateCharacter.id);
            if(character is null)
                throw new NotFoundException($"Character with id '{updateCharacter.id}' not found.");
            character.defense = updateCharacter.defense;
            character.hitpoints = updateCharacter.hitpoints;
            character.intelligence = updateCharacter.intelligence;
            character.strength = updateCharacter.strength;
            await _context.SaveChangesAsync();
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return  serviceResponse;
        }

    
    }
}