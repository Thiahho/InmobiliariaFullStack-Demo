using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using DrCell_V02.Controllers.Base;

namespace DrCell_V02.Controllers
{
    [Route("Celulares")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class CelularesController : BaseCelularController
    {
        public CelularesController(ApplicationDbContext context, IvCelularesInfoService celularesService, ILogger<BaseCelularController> logger)
            : base(context, celularesService, logger)
        {
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetCelulares()
        {
            return await base.GetCelulares();
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetMarcas()
        {
            return await base.GetMarcas();
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetModelos()
        {
            return await base.GetModelos();
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetModelosPorMarca(string marca)
        {
            return await base.GetModelosPorMarca(marca);
        }

        /// <summary>
        /// Endpoint específico para el frontend - modelos por marca
        /// </summary>
        [AllowAnonymous]
        [HttpGet("modelos/{marca}")]
        public async Task<IActionResult> GetModelosPorMarcaLegacy(string marca)
        {
            try
            {
                var modelos = await _celularesService.ObtenerModelosPorMarcaAsync(marca);
                return Ok(modelos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener modelos para la marca {marca}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        //[AllowAnonymous]
        //public override async Task<IActionResult> BuscarCelulares([FromQuery] string termino)
        //{
        //    return await base.BuscarCelulares(termino);
        //}

        ///// <summary>
        ///// Sobrescribe el método base para permitir acceso anónimo
        ///// </summary>
        //[AllowAnonymous]
        //public override async Task<IActionResult> GetCelularById(int id)
        //{
        //    return await base.GetCelularById(id);
        //}
    
        //[HttpGet("modelos/{marca}")]
        //public async Task<IActionResult> GetModelosPorMarca(string marca) 
        //    => Ok(await _celularesService.ObtenerCelularesInfoByMarcaAsync(marca));

        [HttpGet("celulares/buscar/{marca}/{modelo}")]
        public async Task<IActionResult> GetModulosByModelo(string marca, string modelo)
            => Ok(await _celularesService.ObtenerCelularesInfoByModeloYMarcaAsync(marca, modelo));

        [HttpGet("vista-completa")]
        public async Task<IActionResult> GetVistaCompleta()
        {
            try
            {
                var resultado = await _context.vCelularesMBP
                    .Take(50) // Limitar a 50 registros para no sobrecargar
                    .Select(v => new{
                        v.marca,
                        v.modelo,
                        v.arreglomodulo,
                        v.arreglobat,
                        v.arreglopin,
                        v.color,
                        v.tipo,
                        v.placa,
                    })
                    .ToListAsync();
                return Ok(new{
                    success = true,
                    message = "Vista completa obtenida correctamente",
                    count = resultado.Count,
                    data = resultado
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new{
                    success = false,
                    message = "Error al obtener la vista completa",
                    error = ex.Message
                });
            }
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarReparaciones([FromQuery] string? termino = null, [FromQuery] string? marca = null, [FromQuery] string? modelo = null)
        {
            try
            {
                // Intentar usar la vista completa primero
                try
                {
                    var queryVista = _context.vCelularesMBP.AsQueryable();

                    if (!string.IsNullOrEmpty(termino))
                    {
                        queryVista = queryVista.Where(v => 
                            (v.marca != null && EF.Functions.ILike(v.marca, $"%{termino}%")) || 
                            (v.modelo != null && EF.Functions.ILike(v.modelo, $"%{termino}%"))
                        );
                    }

                    if (!string.IsNullOrEmpty(marca))
                    {
                        queryVista = queryVista.Where(v => v.marca == marca);
                    }

                    if (!string.IsNullOrEmpty(modelo))
                    {
                        queryVista = queryVista.Where(v => v.modelo == modelo);
                    }

                    // Usar timeout de 5 segundos para la vista
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    
                    var resultadoVista = await queryVista
                        .Take(100)
                        .Select(v => new
                        {
                            v.marca,
                            v.modelo,
                            v.arreglomodulo,
                            v.arreglobat,
                            v.arreglopin,
                            v.color,
                            v.tipo,
                            v.tipopin,
                            v.placa,
                            v.marco,
                            v.version
                        })
                        .ToListAsync(cts.Token);

                    return Ok(new{
                        success = true,
                        message = "Búsqueda realizada correctamente (vista completa)",
                        count = resultadoVista.Count,
                        filters = new { termino, marca, modelo },
                        data = resultadoVista
                    });
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Timeout en vista vCelularesMBP, usando fallback con tablas individuales");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error en vista vCelularesMBP, usando fallback con tablas individuales");
                }

                // Fallback: usar tablas individuales para obtener datos reales
                var query = _context.Celulares.AsQueryable();

                if (!string.IsNullOrEmpty(termino))
                {
                    query = query.Where(c => 
                        (c.marca != null && EF.Functions.ILike(c.marca, $"%{termino}%")) || 
                        (c.modelo != null && EF.Functions.ILike(c.modelo, $"%{termino}%"))
                    );
                }

                if (!string.IsNullOrEmpty(marca))
                {
                    query = query.Where(c => c.marca == marca);
                }

                if (!string.IsNullOrEmpty(modelo))
                {
                    query = query.Where(c => c.modelo == modelo);
                }

                var celulares = await query.Take(100).ToListAsync();

                // Obtener precios reales de las tablas individuales
                var resultado = new List<object>();
                foreach (var cel in celulares)
                {
                    var modulo = await _context.Modulos
                        .Where(m => m.marca == cel.marca && m.modelo == cel.modelo)
                        .FirstOrDefaultAsync();
                    
                    var bateria = await _context.Baterias
                        .Where(b => b.marca == cel.marca && b.modelo == cel.modelo)
                        .FirstOrDefaultAsync();
                    
                    var pin = await _context.Pines
                        .Where(p => p.marca == cel.marca && p.modelo == cel.modelo)
                        .FirstOrDefaultAsync();

                    resultado.Add(new
                    {
                        marca = cel.marca,
                        modelo = cel.modelo,
                        arreglomodulo = modulo?.arreglo,
                        arreglobat = bateria?.arreglo ?? 40000m,
                        arreglopin = pin?.arreglo,
                        color = modulo?.color,
                        tipo = modulo?.tipo,
                        tipopin = pin?.tipo,
                        placa = pin?.placa,
                        marco = modulo?.marco,
                        version = modulo?.version
                    });
                }

            return Ok(new{
                success = true,
                message = "Busqueda realizada correctamente",
                count = resultado.Count,
                filters = new { termino, marca, modelo },
                data = resultado
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new{
                success = false,
                message = "Error al realizar la busqueda",
                error = ex.Message
            });
            }
        }


        
    }
}
