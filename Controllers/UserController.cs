namespace RPG_dotnet.Controllers;

[Authorize]
[ApiController]
[Route("phantasm/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> GetUsers()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _userService.GetUsersAsync(userId));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> SearchUsers(
        [FromQuery] string query)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var response = await _userService.SearchUsersAsync(userId, query);
        return Ok(response);
    }

}