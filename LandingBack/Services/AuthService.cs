using BCrypt.Net;
using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IJwtService jwtService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest, string ipAddress, string userAgent)
        {
            var agente = await _context.Agentes
                .FirstOrDefaultAsync(a => a.Email == loginRequest.Email);

            if (agente == null)
            {
                await LogFailedAttempt(loginRequest.Email, ipAddress, userAgent, "Usuario no encontrado");
                return null;
            }

            if (agente.BloqueoHasta.HasValue && agente.BloqueoHasta.Value > DateTime.UtcNow)
            {
                await LogFailedAttempt(loginRequest.Email, ipAddress, userAgent, "Usuario bloqueado");
                return null;
            }

            if (!agente.Activo)
            {
                await LogFailedAttempt(loginRequest.Email, ipAddress, userAgent, "Usuario inactivo");
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, agente.Password))
            {
                agente.IntentosFallidosLogin++;
                
                if (agente.IntentosFallidosLogin >= 5)
                {
                    agente.BloqueoHasta = DateTime.UtcNow.AddMinutes(30);
                    await LogFailedAttempt(loginRequest.Email, ipAddress, userAgent, "Usuario bloqueado por intentos fallidos");
                }
                else
                {
                    await LogFailedAttempt(loginRequest.Email, ipAddress, userAgent, "Contraseña incorrecta");
                }

                await _context.SaveChangesAsync();
                return null;
            }

            agente.IntentosFallidosLogin = 0;
            agente.BloqueoHasta = null;
            agente.UltimoLogin = DateTime.UtcNow;
            agente.RefreshToken = _jwtService.GenerateRefreshToken();
            agente.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"]));

            await _context.SaveChangesAsync();

            await LogSuccessfulLogin(agente.Id, ipAddress, userAgent);

            var accessToken = _jwtService.GenerateAccessToken(agente);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = agente.RefreshToken,
                token = accessToken, // Para compatibilidad con frontend
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                Agente = new AgenteDto
                {
                    Id = agente.Id,
                    Nombre = agente.Nombre,
                    Email = agente.Email,
                    Telefono = agente.Telefono,
                    Rol = agente.Rol,
                    Activo = agente.Activo,
                    UltimoLogin = agente.UltimoLogin,
                    FechaCreacion = agente.FechaCreacion
                }
            };
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return null;

            var agente = await _context.Agentes
                .FirstOrDefaultAsync(a => a.Email == email);

            if (agente == null || !_jwtService.ValidateRefreshToken(agente, request.RefreshToken))
                return null;

            var newAccessToken = _jwtService.GenerateAccessToken(agente);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            agente.RefreshToken = newRefreshToken;
            agente.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"]));

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                token = newAccessToken, // Para compatibilidad con frontend
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                Agente = new AgenteDto
                {
                    Id = agente.Id,
                    Nombre = agente.Nombre,
                    Email = agente.Email,
                    Telefono = agente.Telefono,
                    Rol = agente.Rol,
                    Activo = agente.Activo,
                    UltimoLogin = agente.UltimoLogin,
                    FechaCreacion = agente.FechaCreacion
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var agente = await _context.Agentes
                .FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);

            if (agente == null)
                return false;

            agente.RefreshToken = null;
            agente.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LogoutAsync(int agenteId)
        {
            var agente = await _context.Agentes.FindAsync(agenteId);
            if (agente != null)
            {
                agente.RefreshToken = null;
                agente.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }
        }

        private async Task LogFailedAttempt(string email, string ipAddress, string userAgent, string motivo)
        {
            var auditLog = new AuditLog
            {
                Accion = "LOGIN_FAILED",
                Entidad = "Login",
                ValorNuevo = $"Email: {email}, Motivo: {motivo}",
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CreateAdminAsync(CreateAdminDto createAdminDto)
        {
            try
            {
                var adminExists = await GetAdminExistsAsync();
                if (adminExists)
                    return false;

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createAdminDto.Password);

                var admin = new Agente
                {
                    Nombre = createAdminDto.Nombre,
                    Email = createAdminDto.Email,
                    Password = hashedPassword,
                    Telefono = createAdminDto.Telefono,
                    Rol = "Admin",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                _context.Agentes.Add(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Administrador creado exitosamente: {createAdminDto.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear administrador");
                return false;
            }
        }

        public async Task<bool> GetAdminExistsAsync()
        {
            return await _context.Agentes.AnyAsync(a => a.Rol == "Admin");
        }

        private async Task LogSuccessfulLogin(int agenteId, string ipAddress, string userAgent)
        {
            var auditLog = new AuditLog
            {
                Accion = "LOGIN_SUCCESS",
                Entidad = "Login",
                AgenteId = agenteId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}