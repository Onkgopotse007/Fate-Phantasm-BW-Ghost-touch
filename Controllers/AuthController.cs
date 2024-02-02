using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RPG_dotnet.Controllers
{
    [ApiController]
    
    [Route("phantasm/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [AllowUnauthenticated]
        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<UserRegistrationResponse>>> Register(UserRegisterDto request,
        [FromServices] IValidator<UserRegisterDto> validator)
        {
            Functions functions = new Functions();
            await functions.validateDtoAsync(request, validator);
            var response = await _authService.Register(
                new User { userName = request.userName }, request.password
            );
            return CreatedAtAction(nameof(Register), response);
        }
        [AllowUnauthenticated]
        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<UserLoginResponse>>> Login(UserLoginDto request,
        [FromServices] IValidator<UserLoginDto> validator)
        {
            Functions functions = new Functions();
            await functions.validateDtoAsync(request, validator);
            var response = await _authService.Login(
                request.userName, request.password
            );
            return Ok(response);
        }
    }
}