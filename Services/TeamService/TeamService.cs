using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Services.TeamService
{
    public class TeamService : ITeamService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public TeamService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AssignCharacterToUser(int userId,int characterId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.id.Equals(userId));
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.id.Equals(characterId));
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var userCharacters = await _context.UserCharacters
                .Where(uc => uc.userId == userId)
                .ToListAsync();
            if (user == null)
                throw new NotFoundException("User not found");

            if (character == null)
                throw new NotFoundException("Character does not exist");

            if (userCharacters.Count >= 7)
                throw new ConflictException("Cannot have more than 7 heroic spirits");

            if (await _context.UserCharacters.AnyAsync(uc => uc.userId == userId && uc.character.fighterClass == character.fighterClass))
                throw new ConflictException("Cannot have more than 1 heroic spirit of the same class");

            var userCharacter = new UserCharacter
            {
                userId = userId,
                user = user,
                characterId = characterId,
                character = character
            };

            _context.UserCharacters.Add(userCharacter);
            await _context.SaveChangesAsync();
            var updatedUserCharacters = await _context.UserCharacters
            .Where(uc => uc.userId == userId)
            .ToListAsync();
            var postedCharacters = await _context.UserCharacters
                .Include(uc => uc.character)
                .Where(uc => uc.userId == userId)
                .ToListAsync();

            serviceResponse.data = postedCharacters
                .Select(uc => _mapper.Map<GetCharacterDto>(uc.character))
                .ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetUserCharacter(int userId, int characterId)
        {
            var userCharacters = await _context.UserCharacters
                .Include(uc => uc.character)
                .Where(uc => uc.userId == userId && uc.characterId == characterId)
                .ToListAsync();
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            if(userCharacters.Count==0)
                throw new NotFoundException("User does not have any characters matching your query");

            serviceResponse.message="Charcters found";
            serviceResponse.data = userCharacters.Select(uc=>_mapper.Map<GetCharacterDto>(uc)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetUserCharacters(int userId)
        {
            var userCharacters = await _context.UserCharacters
                .Include(uc => uc.character)
                .Where(uc => uc.userId == userId)
                .ToListAsync();
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            if(userCharacters.Count==0)
                throw new NotFoundException("User does not have any characters");

            serviceResponse.message="Charcters found";
            serviceResponse.data = userCharacters.Select(uc=>_mapper.Map<GetCharacterDto>(uc)).ToList();
            return serviceResponse;
        }
    }
}