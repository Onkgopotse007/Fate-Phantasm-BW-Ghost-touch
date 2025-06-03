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

            var character = _mapper.Map<Characters>(newCharacter);

            // Assign abilities directly
            character.abilities = newCharacter.abilities
                .Select(a => new Ability
                {
                    name = a.name,
                    description = a.description,
                    manaCost = a.manaCost,
                    damage = a.damage
                }).ToList();

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            serviceResponse.data = await _context.Characters
                .Include(c => c.abilities)
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

            var character = await _context.Characters
                .Include(c => c.abilities)
                .FirstOrDefaultAsync(c => c.id == id);

            serviceResponse.data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }


        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var characters = await _context.Characters
                .Include(c => c.abilities)
                .ToListAsync();

            return new ServiceResponse<List<GetCharacterDto>>
            {
                data = _mapper.Map<List<GetCharacterDto>>(characters)
            };
        }


        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();

            var character = await _context.Characters
                .Include(c => c.abilities)
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

            // Replace abilities
            if (updateCharacter.abilities is { Count: > 0 })
            {
                _context.Abilities.RemoveRange(character.abilities);

                character.abilities = updateCharacter.abilities
                    .Select(a => new Ability
                    {
                        name = a.name,
                        description = a.description,
                        manaCost = a.manaCost,
                        damage = a.damage
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