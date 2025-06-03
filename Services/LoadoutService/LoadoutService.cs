using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPG_dotnet.Helpers;
namespace RPG_dotnet.Services.LoadoutService
{
    public class LoadoutService : ILoadoutService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public LoadoutService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<GetLoadoutDto>> CreateLoadoutAsync(int userId, CreateLoadoutDto dto)
        {
            var response = new ServiceResponse<GetLoadoutDto>();

            var existingCount = await _context.Loadouts.CountAsync(l => l.userId == userId);
            if (existingCount >= 3)
            {
                throw new BadRequestException("User cannot have more than 3 loadouts.");
            }

            var nameExists = await _context.Loadouts
                .AnyAsync(l => l.userId == userId && l.name == dto.name);

            if (nameExists)
            {
                throw new BadRequestException($"You already have a loadout named '{dto.name}'.");
            }

            var characters = await _context.Characters
                .Where(c => dto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
            {
                throw new BadRequestException("Exactly 2 valid characters must be provided.");
            }

            if (!characters.Any(c => c.role == RoleType.Vanguard) ||
                !characters.Any(c => c.role == RoleType.Support))
            {
                throw new BadRequestException("Loadout must contain one Vanguard and one Support character.");
            }
            var loadout = new Loadout
            {
                userId = userId,
                name = dto.name,
                characters = characters.Select(c => new LoadoutCharacter
                {
                    characterId = c.id
                }).ToList()
            };

            _context.Loadouts.Add(loadout);
            await _context.SaveChangesAsync();

            response.data = _mapper.Map<GetLoadoutDto>(loadout);
            return response;
        }

        public async Task<ServiceResponse<GetLoadoutDto>> UpdateLoadoutAsync(int userId, int loadoutId, UpdateLoadoutDto dto)
        {
            var response = new ServiceResponse<GetLoadoutDto>();

            var loadout = await _context.Loadouts
                .Include(l => l.characters)
                .FirstOrDefaultAsync(l => l.loadoutId == loadoutId && l.userId == userId);

            if (loadout == null)
            {
                throw new BadRequestException("Loadout not found.");
            }

            // Ensure name is unique per user (excluding the current loadout)
            var nameExists = await _context.Loadouts
                .AnyAsync(l => l.userId == userId && l.name == dto.name && l.loadoutId != loadoutId);

            if (nameExists)
            {
                throw new BadRequestException($"You already have a loadout named '{dto.name}'.");
            }

            var characters = await _context.Characters
                .Where(c => dto.characterIds.Contains(c.id))
                .ToListAsync();

            if (characters.Count != 2)
            {
                throw new BadRequestException("Exactly 2 valid characters must be provided.");
            }

            if (!characters.Any(c => c.role == RoleType.Vanguard) ||
                !characters.Any(c => c.role == RoleType.Support))
            {
                throw new BadRequestException("Loadout must contain one Vanguard and one Support character.");
            }

            loadout.name = dto.name;
            loadout.characters = characters.Select(c => new LoadoutCharacter
            {
                characterId = c.id
            }).ToList();

            await _context.SaveChangesAsync();

            response.data = _mapper.Map<GetLoadoutDto>(loadout);
            return response;
        }


        public async Task<ServiceResponse<List<GetLoadoutDto>>> GetUserLoadoutsAsync(int userId)
        {
            var loadouts = await _context.Loadouts
                .Where(l => l.userId == userId)
                .Include(l => l.characters)
                .ThenInclude(lc => lc.character)
                .ToListAsync();

            return new ServiceResponse<List<GetLoadoutDto>>
            {
                data = _mapper.Map<List<GetLoadoutDto>>(loadouts)
            };
        }
    }
}