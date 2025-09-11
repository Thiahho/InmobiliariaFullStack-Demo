using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers.UserAgent.ToString();

            var result = await _authService.LoginAsync(loginRequest, ipAddress, userAgent);

            if (result == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Token inválido" });

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token requerido" });

            var result = await _authService.RevokeTokenAsync(refreshToken);

            if (!result)
                return BadRequest(new { message = "Token inválido" });

            return Ok(new { message = "Token revocado exitosamente" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var agenteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(agenteIdClaim, out int agenteId))
            {
                await _authService.LogoutAsync(agenteId);
            }

            return Ok(new { message = "Logout exitoso" });
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto createAdminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAdmin = await _authService.GetAdminExistsAsync();
            if (existingAdmin)
                return BadRequest(new { message = "Ya existe un usuario administrador" });

            var result = await _authService.CreateAdminAsync(createAdminDto);
            if (!result)
                return BadRequest(new { message = "Error al crear el administrador" });

            return Ok(new { message = "Administrador creado exitosamente" });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var agenteId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nombre = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;
                var activo = User.FindFirst("activo")?.Value;
                var telefono = User.FindFirst("telefono")?.Value;

                return Ok(new
                {
                    id = agenteId,
                    nombre = nombre ?? "Admin",
                    email = email ?? "admin@inmobiliaria.com",
                    rol = rol ?? "Admin",
                    activo = bool.Parse(activo ?? "true"),
                    telefono = telefono
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}