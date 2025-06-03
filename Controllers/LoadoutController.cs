using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RPG_dotnet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoadoutController : ControllerBase
    {
        private readonly ILoadoutService _service;

        public LoadoutController(ILoadoutService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoadout(CreateLoadoutDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _service.CreateLoadoutAsync(userId, dto);
            if (!result.success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyLoadouts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _service.GetUserLoadoutsAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLoadout(UpdateLoadoutDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _service.UpdateLoadoutAsync(userId, dto.loadoutId, dto);
            if (!result.success) return BadRequest(result);
            return Ok(result);
        }
    }
}