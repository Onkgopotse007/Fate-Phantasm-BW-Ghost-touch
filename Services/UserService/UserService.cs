namespace RPG_dotnet.Services.UserService;

public class UserService: IUserService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<List<GetUserDto>>> GetUsersAsync(int requestingUserId)
    {
        var users = await _context.Users
            .Where(u => u.id != requestingUserId)
            .OrderBy(u => u.userName)
            .ToListAsync();

        return ServiceResponse<List<GetUserDto>>.Success(
            users.Select(u => _mapper.Map<GetUserDto>(u)).ToList(),
            "Users retrieved successfully");
    }

    public async Task<ServiceResponse<List<GetUserDto>>> SearchUsersAsync(
        int requestingUserId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ServiceResponse<List<GetUserDto>>.Success(
                new List<GetUserDto>(), "No query provided");

        var users = await _context.Users
            .Where(u => u.id != requestingUserId &&
                        u.userName.ToLower().Contains(query.ToLower()))
            .OrderBy(u => u.userName)
            .ToListAsync();

        if (!users.Any())
            throw new NotFoundException($"No users found matching '{query}'.");

        return ServiceResponse<List<GetUserDto>>.Success(
            users.Select(u => _mapper.Map<GetUserDto>(u)).ToList(),
            "Users retrieved successfully");
    }

}