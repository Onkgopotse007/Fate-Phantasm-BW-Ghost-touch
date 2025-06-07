namespace RPG_dotnet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("phantasm/[controller]")]
    public class GameSessionController : ControllerBase
    {
        public IGameSessionService _gameSessionService;

        public GameSessionController(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> CreateGameSession(
            CreateGameSessionDto newSessionDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.CreateGameSessionAsync(userId, newSessionDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<ActionResult<ServiceResponse<List<GetGameSessionDto>>>> GetActiveGameSessions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.GetActiveGameSessionsAsync(userId);
            return Ok(response);
        }

        [HttpGet("{sessionId}")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> GetGameSessionById(int sessionId)
        {
            var response = await _gameSessionService.GetGameSessionByIdAsync(sessionId);
            if (response.data is null)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<GetGameSessionDto>>>> GetGameSessionsByUserId(GameSessionState? state = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.GetGameSessionsByUserIdAsync(userId, state);
            return Ok(response);
        }

        [HttpPut("move")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> MoveCharacter(
            MoveActionDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.MoveCharacterAsync(userId, dto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("attack")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> AttackCharacter(
            AttackCharacterDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.AttackCharacterAsync(userId, dto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("cast")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> CastSpell(
            CastSpellDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.CastSpellAsync(userId, dto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("abandon")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> AbandonSession(int sessionId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.AbandonSessionAsync(userId, sessionId);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("accept")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> AcceptSession(AcceptGameSessionDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.AcceptSessionAsync(userId, dto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("reject")]
        public async Task<ActionResult<ServiceResponse<GetGameSessionDto>>> RejectSession(int sessionId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _gameSessionService.RejectSessionAsync(userId, sessionId);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
