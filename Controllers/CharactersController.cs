using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

using RPG_dotnet.Helpers;

namespace RPG_dotnet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("phantasm/[controller]")]
    public class CharactersController : ControllerBase
    {
        public ICharacterService _characterService;

        public CharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> Get()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c=>c.Type== ClaimTypes.NameIdentifier)!.Value);
            return Ok(await _characterService.GetCharacters(userId));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> Get(int id)
        {
            var response = await _characterService.GetCharacterById(id);
            if (response.data is null)
                return NotFound(response);
            return Ok(await _characterService.GetCharacterById(id));
        }
        [Authorize(Roles ="1")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> CreateCharacter(AddCharacterDto newCharacter,
        [FromServices] IValidator<AddCharacterDto> validator)
        {
            Functions functions = new Functions();
            await functions.validateDtoAsync(newCharacter, validator);
            return Ok(await _characterService.AddCharacter(newCharacter));
        }
        
        [HttpPut]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> UpdateCharacter(UpdateCharacterDto updatedCharacter,
        [FromServices] IValidator<UpdateCharacterDto> validator)
        {
            Functions functions = new Functions();
            await functions.validateDtoAsync(updatedCharacter, validator);
            return Ok(await _characterService.UpdateCharacter(updatedCharacter));
        }
        [Authorize(Roles ="1")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> DeleteCharacter(int id)
        {
            return Ok(await _characterService.DeleteCharacters(id));
        }

    }
}