namespace RPG_dotnet.Services.UserService;

public interface IUserService
{
    Task<ServiceResponse<List<GetUserDto>>> GetUsersAsync(int requestingUserId);
    Task<ServiceResponse<List<GetUserDto>>> SearchUsersAsync(int requestingUserId, string query);
}