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
        // [EnableRateLimiting("LoginPolicy")] // Commented out until rate limiting is configured in Program.cs
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

        [HttpGet("admin-exists")]
        public async Task<IActionResult> AdminExists()
        {
            var exists = await _authService.GetAdminExistsAsync();
            return Ok(new { adminExists = exists });
        }

        [HttpGet("debug-headers")]
        public IActionResult DebugHeaders()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var allHeaders = Request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
            
            return Ok(new 
            { 
                authorizationHeader = authHeader,
                allHeaders = allHeaders,
                isAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                claims = User?.Claims?.Select(c => $"{c.Type}: {c.Value}")
            });
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
                    nombre = nombre,
                    email = email,
                    rol = rol,
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