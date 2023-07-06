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
        private static List<Characters> characters = new List<Characters>{
            new Characters(),
            new Characters{id=1 ,name="King Arthur", fighterClass= RpgClass.Saber}
        };
        [HttpGet]
        public ActionResult<List<Characters>> Get(){
            return Ok(characters);
        }
        [HttpGet("{id}")]
        public ActionResult<Characters> Get(int id){

            return Ok(characters.First(c => c.id == id));
        }
        [HttpPost]
        public ActionResult<List<Characters>> CreateCharacter(Characters newCharacter){
            characters.Add(newCharacter);
            return Ok(characters);
        }
        [HttpPut]
        public ActionResult<List<Characters>> UpdateCharacter(Characters newCharacter){
            characters.Add(newCharacter);
            return Ok(characters);
        }
    }
}