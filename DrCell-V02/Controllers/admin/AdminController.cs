using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.RateLimiting;
using DrCell_V02.Middleware;
namespace DrCell_V02.Controllers.admin
{
    [Route("Admin")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    [EnableRateLimiting("AuthPolicy")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUsuarioService _usuarioService;
        private readonly ICelularesService _celularesService;
        public AdminController(ApplicationDbContext context, IConfiguration config, IUsuarioService usuarioService, ICelularesService equiposService)
        {
            _context = context;
            _configuration = config;
            _usuarioService = usuarioService;
            _celularesService = equiposService;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        [RateLimit("registro", 2, 10)]
        public async Task<IActionResult> CrearAdmin([FromBody] Usuario usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.ClaveHash))
                {
                    return BadRequest(new { message = "Email y contraseña son requeridos" });
                }

                usuario.Rol = "ADMIN";
                var usuarioCreado = await _usuarioService.CrearUsuarioAsync(usuario);

                return Ok(new
                {
                    message = "Administrador creado correctamente",
                    usuario = new
                    {
                        id = usuarioCreado.Id,
                        email = usuarioCreado.Email,
                        rol = usuarioCreado.Rol
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el administrador", error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [RateLimit("login", 3, 5)]
        public async Task<IActionResult> Login([FromBody] Auth auth)
        {
            try
            {
                if (string.IsNullOrEmpty(auth.Email) || string.IsNullOrEmpty(auth.Password))
                {
                    return BadRequest("Email y contraseña son requeridos");
                }

                var usuario = await _usuarioService.ValidarCredencialesAsync(auth.Email, auth.Password);

                if (usuario == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                if (usuario.Rol?.ToUpper() != "ADMIN")
                {
                    return Unauthorized(new { message = "No tienes permisos de administrador" });
                }

                var token = _usuarioService.GenerarToken(usuario);

                // 🔧 FIX: Configuración de cookie mejorada para desarrollo
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(24), // Expira en 24 horas
                    Path = "/"
                };

                // 🔧 DEBUG: Verificar configuración de ambiente
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                Console.WriteLine($"🔧 DEBUG: Ambiente detectado: '{environment}'");
                
                // Configuración específica por ambiente
                if (environment == "Development")
                {
                    cookieOptions.HttpOnly = false; // Permitir acceso desde JS en desarrollo
                    cookieOptions.Secure = false; // HTTP permitido en desarrollo
                    cookieOptions.SameSite = SameSiteMode.Lax; // Lax para compatibilidad cross-origin
                    Console.WriteLine($"🔧 DEBUG: Usando configuración de DESARROLLO");
                }
                else
                {
                    cookieOptions.HttpOnly = true; // Seguro en producción
                    cookieOptions.Secure = true; // Solo HTTPS en producción
                    cookieOptions.SameSite = SameSiteMode.Strict; // Estricto en producción
                    Console.WriteLine($"🔧 DEBUG: Usando configuración de PRODUCCIÓN");
                }

                // 🔧 DEBUG: Agregar logs para verificar cookie
                Console.WriteLine($"🔧 DEBUG: Intentando establecer cookie AuthToken");
                Console.WriteLine($"🔧 DEBUG: Token generado: {token?.Substring(0, 20)}...");
                Console.WriteLine($"🔧 DEBUG: Cookie options - Secure: {cookieOptions.Secure}, SameSite: {cookieOptions.SameSite}");
                
                if (!string.IsNullOrEmpty(token))
                {
                    Response.Cookies.Append("AuthToken", token, cookieOptions);
                    Console.WriteLine($"🔧 DEBUG: Cookie establecida exitosamente");
                }
                else
                {
                    Console.WriteLine($"🔧 ERROR: Token es null o vacío, no se puede establecer cookie");
                }

                return Ok(new
                {
                    message = "Inicio de sesión exitoso",
                    usuario = new
                    {
                        id = usuario.Id,
                        email = usuario.Email,
                        rol = usuario.Rol
                    },
                    token = token, // 🔧 TEMP: Enviar token en respuesta para desarrollo
                    debug = new // 🔧 DEBUG: Agregar info de debug
                    {
                        tokenLength = token?.Length,
                        cookieSecure = cookieOptions.Secure,
                        cookieSameSite = cookieOptions.SameSite.ToString(),
                        cookieHttpOnly = cookieOptions.HttpOnly,
                        environment = _configuration["ASPNETCORE_ENVIRONMENT"],
                        cookieExpires = cookieOptions.Expires
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            try
            {
                Console.WriteLine($"🔧 DEBUG: Iniciando proceso de logout");
                Console.WriteLine($"🔧 DEBUG: Cookies antes del logout: {string.Join(", ", Request.Cookies.Select(c => c.Key))}");

                // Configurar las mismas opciones de cookie que se usaron en el login
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1), // Fecha pasada para expirar inmediatamente
                    Path = "/"
                };

                // Aplicar la misma configuración por ambiente que en login
                if (environment == "Development")
                {
                    cookieOptions.HttpOnly = false;
                    cookieOptions.Secure = false;
                    cookieOptions.SameSite = SameSiteMode.Lax;
                }
                else
                {
                    cookieOptions.HttpOnly = true;
                    cookieOptions.Secure = true;
                    cookieOptions.SameSite = SameSiteMode.Strict;
                }

                // Eliminar cookie de autenticación con las opciones correctas
                Response.Cookies.Delete("AuthToken", cookieOptions);
                
                // También intentar eliminar con configuraciones alternativas por si acaso
                Response.Cookies.Append("AuthToken", "", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                    Path = "/",
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.None
                });

                Console.WriteLine($"🔧 DEBUG: Cookie AuthToken eliminada");

                return Ok(new { 
                    message = "Sesión cerrada exitosamente",
                    debug = new
                    {
                        environment = environment,
                        cookieDeleted = true,
                        cookieOptions = new
                        {
                            httpOnly = cookieOptions.HttpOnly,
                            secure = cookieOptions.Secure,
                            sameSite = cookieOptions.SameSite.ToString()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔧 ERROR: Error al cerrar sesión: {ex.Message}");
                return StatusCode(500, new { message = "Error al cerrar sesión", error = ex.Message });
            }
        }

        [HttpGet("verify")]
        [AllowAnonymous]
        public IActionResult VerifySession()
        {
            try
            {
                Console.WriteLine($"🔍 DEBUG: Verificando sesión...");
                Console.WriteLine($"🔍 DEBUG: User.Identity.IsAuthenticated = {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"🔍 DEBUG: Cookies: {string.Join(", ", Request.Cookies.Select(c => $"{c.Key}={c.Value?.Substring(0, Math.Min(20, c.Value.Length))}..."))}");
                Console.WriteLine($"🔍 DEBUG: Authorization Header: {Request.Headers.Authorization}");

                // Verificar si el usuario está autenticado (ya sea por cookie o por header Authorization)
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var email = User.FindFirst(ClaimTypes.Email)?.Value;
                    var role = User.FindFirst(ClaimTypes.Role)?.Value;

                    Console.WriteLine($"✅ DEBUG: Usuario autenticado - ID: {userId}, Email: {email}, Role: {role}");

                    return Ok(new
                    {
                        isAuthenticated = true,
                        usuario = new
                        {
                            id = userId,
                            email = email,
                            rol = role
                        }
                    });
                }

                Console.WriteLine($"❌ DEBUG: Usuario no autenticado");
                return Ok(new { isAuthenticated = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DEBUG: Error en verify: {ex.Message}");
                return StatusCode(500, new { message = "Error al verificar sesión", error = ex.Message });
            }
        }

        private async Task<bool> AlreadyExist(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // ============================= ENDPOINTS PROTEGIDOS =============================


        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetCelulares()
        {
            try
            {
                var celulares = await _celularesService.ObtenerEquiposUnicosAsync();
                return Ok(celulares);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener los celulares", error = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("marcas")]
        public async Task<IActionResult> GetMarcas()
        {
            try
            {
                var marcas = await _celularesService.ObtenerMarcasAsync();
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener las marcas", error = ex.Message });
            }
        }

        [HttpGet("modelos")]
        public async Task<IActionResult> GetModelos()
        {
            try
            {
                var modelos = await _celularesService.ObtenerModelosAsync();
                return Ok(modelos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener los modelos", error = ex.Message });
            }
        }

        [HttpGet("modelos/{marca}")]
        public async Task<IActionResult> GetModelosPorMarca(string marca)
        {
            try
            {
                var marcas = await _celularesService.ObtenerModelosPorMarcaAsync(marca);
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener los modelos por marca", error = ex.Message });
            }

        }

        [HttpGet("info/{marca}/{modelo}")]
        public async Task<IActionResult> GetModulosByModelo(string marca, string modelo)
        {
            try
            {
                var info = await _celularesService.ObtenerInfoPorMarcaYModeloAsync(marca, modelo);
                if (info == null || !info.Any())
                {
                    return NotFound(new { message = "No se encontraron resultados para la marca y modelo especificados." });
                }
                return Ok(info);
            }
            catch
            {
                return BadRequest(new { message = "Error al obtener la información por marca y modelo." });
            }
        }

    }
}
