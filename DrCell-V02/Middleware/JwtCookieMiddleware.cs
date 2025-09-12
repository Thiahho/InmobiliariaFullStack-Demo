using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DrCell_V02.Middleware
{
    /// <summary>
    /// Middleware para manejar autenticación JWT con cookies httpOnly
    /// Extrae el token JWT de las cookies y lo agrega al header Authorization
    /// </summary>
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtCookieMiddleware> _logger;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtCookieMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                string? token = null;
                string source = "";

                // 1. Verificar si ya hay Authorization header
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                    source = "Authorization Header";
                    Console.WriteLine($"🔍 DEBUG: Token encontrado en Authorization Header");
                }
                // 2. Si no hay header, verificar cookies
                else if (context.Request.Cookies.TryGetValue("AuthToken", out var cookieToken))
                {
                    token = cookieToken;
                    source = "Cookie";
                    Console.WriteLine($"🔍 DEBUG: Token encontrado en Cookie");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"🔍 DEBUG: Validando token de {source}...");
                    
                    // Validar el token JWT
                    if (ValidateJwtToken(token))
                    {
                        // Solo agregar header si no existe ya
                        if (string.IsNullOrEmpty(authHeader))
                        {
                            context.Request.Headers.Add("Authorization", $"Bearer {token}");
                            Console.WriteLine($"✅ DEBUG: Token válido de {source} agregado al header Authorization");
                        }
                        else
                        {
                            Console.WriteLine($"✅ DEBUG: Token válido de {source} confirmado");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ DEBUG: Token inválido de {source}");
                        _logger.LogWarning($"Token JWT en {source} es inválido o expirado");
                        
                        // Limpiar cookie inválida si el token vino de cookie
                        if (source == "Cookie")
                        {
                            context.Response.Cookies.Delete("AuthToken");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"🔍 DEBUG: No se encontró token en cookies ni en Authorization header");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DEBUG: Error en JwtCookieMiddleware: {ex.Message}");
                _logger.LogError(ex, "Error al procesar token JWT");
            }

            await _next(context);
        }

        /// <summary>
        /// Valida si el token JWT es válido
        /// </summary>
        private bool ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "");
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Token JWT inválido: {Message}", ex.Message);
                return false;
            }
        }
    }
} 