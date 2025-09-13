using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuariosService usuariosService, ILogger<UsuariosController> logger)
        {
            _usuariosService = usuariosService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _usuariosService.GetUsuariosAsync();
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateAgenteDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            int.TryParse(adminIdStr, out var adminId);
            var user = await _usuariosService.CreateAgenteAsync(dto, adminId);
            return Ok(user);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            int.TryParse(userIdStr, out var userId);
            var user = await _usuariosService.UpdateProfileAsync(userId, dto);
            return Ok(user);
        }

        [HttpPost("{id}/toggle-activo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var ok = await _usuariosService.ToggleActivoAsync(id);
            if (!ok) return NotFound();
            return Ok(new { success = true });
        }
    }
}


