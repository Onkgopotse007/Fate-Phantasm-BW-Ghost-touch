using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RPG_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        public ICharacterService _characterService;

        public CharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Characters>>>> Get(){
            return Ok(await _characterService.GetCharacters());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Characters>>> Get(int id){
            return Ok(await _characterService.GetCharacterById(id));
        }
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<Characters>>>> CreateCharacter(Characters newCharacter){
            return Ok(await _characterService.AddCharacter(newCharacter));
        }
    }
}