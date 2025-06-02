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

            if (await CharacterExists(newCharacter.name))
            {
                throw new ConflictException($"Character {newCharacter.name} already exists");
            }

            // Validate ability IDs
            var abilities = await _context.Abilities
                .Where(a => newCharacter.abilityIds.Contains(a.id))
                .ToListAsync();

            if (abilities.Count != newCharacter.abilityIds.Count)
            {
                throw new BadRequestException("One or more provided abilityIds are invalid.");
            }

            // Map base character
            var character = _mapper.Map<Characters>(newCharacter);

            // Link abilities via join table
            character.characterAbilities = abilities
                .Select(a => new CharacterAbility
                {
                    ability = a,
                    character = character
                }).ToList();

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            // Return updated character list
            serviceResponse.data = await _context.Characters
                .Include(c => c.characterAbilities)
                .ThenInclude(ca => ca.ability)
                .Select(c => _mapper.Map<GetCharacterDto>(c))
                .ToListAsync();

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacters(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _context.Characters.FirstOrDefault(c => c.id == id);
            if (character is null)
                throw new NotFoundException($"Character with id '{id}' not found.");
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();
            serviceResponse.data = await _context.Characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.id == id);
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetCharacters(int userId)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var characters = await _context.Characters.ToListAsync();
            serviceResponse.data = characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = await _context.Characters
                .Include(c => c.characterAbilities)
                .FirstOrDefaultAsync(c => c.id == updateCharacter.id);

            if (character is null)
            {
                throw new NotFoundException($"Character with id '{updateCharacter.id}' not found.");
            }

            character.hitpoints = updateCharacter.hitpoints;
            character.mana = updateCharacter.mana;
            character.movement = updateCharacter.movement;
            character.strength = updateCharacter.strength;
            character.defense = updateCharacter.defense;
            character.intelligence = updateCharacter.intelligence;

            // Replace abilities if any are given
            if (updateCharacter.abilityIds is { Count: > 0 })
            {
                var abilities = await _context.Abilities
                    .Where(a => updateCharacter.abilityIds.Contains(a.id))
                    .ToListAsync();

                if (abilities.Count != updateCharacter.abilityIds.Count)
                {
                    throw new BadRequestException("One or more provided abilityIds are invalid.");
                }

                // Remove old mappings
                _context.CharacterAbilities.RemoveRange(character.characterAbilities);

                // Add new mappings
                character.characterAbilities = abilities
                    .Select(a => new CharacterAbility
                    {
                        characterId = character.id,
                        abilityId = a.id
                    }).ToList();
            }

            await _context.SaveChangesAsync();
            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<bool> CharacterExists(string name)
        {
            if (await _context.Characters.AnyAsync(c => c.name.ToLower() == name.ToLower()))
            {
                return true;
            }
            return false;
        }


    }
}