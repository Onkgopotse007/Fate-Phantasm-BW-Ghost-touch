using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RPG_dotnet.Services.LoadoutService
{
    public interface ILoadoutService
    {
        Task<ServiceResponse<GetLoadoutDto>> CreateLoadoutAsync(int userId, CreateLoadoutDto dto);
        Task<ServiceResponse<List<GetLoadoutDto>>> GetUserLoadoutsAsync(int userId);
        Task<ServiceResponse<GetLoadoutDto>> UpdateLoadoutAsync(int userId, int loadoutId, UpdateLoadoutDto dto);
    }
}