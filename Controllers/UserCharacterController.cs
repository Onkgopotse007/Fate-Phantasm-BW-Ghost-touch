using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace RPG_dotnet.Controllers
{
    [Authorize]
    [SetUserIdFilter] 
    [ApiController]
    [Route("phantasm/[controller]")]
    public class UserCharacterController : ControllerBase
    {
        private readonly ITeamService _teamService;
        public int UserId { get; set; }
        public UserCharacterController(ITeamService teamService)
        {
            _teamService = teamService;
        }
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> AssignCharacterToUser(int characterId){
            
            return Ok(await _teamService.AssignCharacterToUser(UserId, characterId));
        }
        [HttpGet("UserCharacters")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> GetUserCharacters(){
            return Ok(await _teamService.GetUserCharacters(UserId));
        }
        [HttpGet("UserCharacter")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> GetUserCharacter(int characterId){
            return Ok(await _teamService.GetUserCharacter(UserId, characterId));
        }
    }
}