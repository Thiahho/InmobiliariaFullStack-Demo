using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.RateLimiting;
namespace DrCell_V02.Controllers
{
    /// <summary>
    /// Controlador para gestionar consultas de reparaciones de celulares
    /// </summary>
    [Route("Reparaciones")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AdminReparacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IvCelularesInfoService _celularesService;
        private readonly ILogger<AdminReparacionesController> _logger;

        public AdminReparacionesController(
            ApplicationDbContext context, 
            IvCelularesInfoService celularesService,
            ILogger<AdminReparacionesController> logger)
        {
            _context = context;
            _celularesService = celularesService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene información general de todos los celulares
        /// </summary>
        /// <returns>Lista de información de celulares</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCelulares()
        {
            try
            {
                _logger.LogInformation("Obteniendo información general de celulares");
                var resultado = await _celularesService.ObtenerCelularesInfoAsync();
                return Ok(new { success = true, data = resultado, count = resultado.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de celulares");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las marcas disponibles
        /// </summary>
        /// <returns>Lista de marcas</returns>
        [HttpGet("marcas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMarcas()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de marcas");
                var marcas = await _celularesService.ObtenerMarcasAsync();
                return Ok(new { success = true, data = marcas, count = marcas.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los modelos disponibles
        /// </summary>
        /// <returns>Lista de modelos</returns>
        [HttpGet("modelos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelos()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de modelos");
                var modelos = await _celularesService.ObtenerModelosAsync();
                return Ok(new { success = true, data = modelos, count = modelos.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modelos");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene modelos por marca específica
        /// </summary>
        /// <param name="marca">Nombre de la marca</param>
        /// <returns>Lista de modelos de la marca</returns>
        [HttpGet("modelos/{marca}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelosPorMarca(string marca)
        {
            if (string.IsNullOrWhiteSpace(marca))
            {
                return BadRequest(new { success = false, message = "La marca es requerida" });
            }

            try
            {
                _logger.LogInformation("Obteniendo modelos para la marca: {Marca}", marca);
                var modelos = await _celularesService.ObtenerModelosPorMarcaAsync(marca);
                return Ok(new { success = true, data = modelos, count = modelos.Count, marca });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modelos para la marca: {Marca}", marca);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene información detallada por marca y modelo
        /// </summary>
        /// <param name="marca">Nombre de la marca</param>
        /// <param name="modelo">Nombre del modelo</param>
        /// <returns>Información detallada del dispositivo</returns>
        [HttpGet("celulares/buscar/{marca}/{modelo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModulosByModelo(string marca, string modelo)
        {
            if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
            {
                return BadRequest(new { success = false, message = "Marca y modelo son requeridos" });
            }

            try
            {
                _logger.LogInformation("Obteniendo información para {Marca} {Modelo}", marca, modelo);
                var resultado = await _celularesService.ObtenerInfoPorMarcaYModeloAsync(marca, modelo);
                return Ok(new { success = true, data = resultado, count = resultado.Count, marca, modelo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información para {Marca} {Modelo}", marca, modelo);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene vista completa de celulares con límite de registros
        /// </summary>
        /// <param name="limit">Límite de registros a retornar (máximo 100)</param>
        /// <returns>Vista completa de celulares</returns>
        [HttpGet("vista-completa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVistaCompleta([FromQuery] int limit = 50)
        {
            try
            {
                // Validar límite
                if (limit <= 0 || limit > 100)
                {
                    limit = 50;
                }

                _logger.LogInformation("Obteniendo vista completa con límite: {Limit}", limit);
                
                var resultado = await _context.vCelularesMBP
                    .Take(limit)
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
                        v.marco,
                        v.placa,
                        v.version
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Vista completa obtenida correctamente",
                    count = resultado.Count,
                    limit,
                    data = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vista completa");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca reparaciones con filtros opcionales
        /// </summary>
        /// <param name="termino">Término de búsqueda general</param>
        /// <param name="marca">Filtro por marca específica</param>
        /// <param name="modelo">Filtro por modelo específico</param>
        /// <param name="limit">Límite de resultados (máximo 100)</param>
        /// <returns>Resultados de búsqueda</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BuscarReparaciones(
            [FromQuery] string? termino = null, 
            [FromQuery] string? marca = null, 
            [FromQuery] string? modelo = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                // Validar límite
                if (limit <= 0 || limit > 100)
                {
                    limit = 100;
                }

                _logger.LogInformation("Buscando reparaciones - Término: {Termino}, Marca: {Marca}, Modelo: {Modelo}, Límite: {Limit}", 
                    termino, marca, modelo, limit);

                var query = _context.vCelularesMBP.AsQueryable();

                // Búsqueda por término general (marca o modelo)
                if (!string.IsNullOrEmpty(termino))
                {
                    query = query.Where(v => 
                        (v.marca != null && EF.Functions.ILike(v.marca, $"%{termino}%")) || 
                        (v.modelo != null && EF.Functions.ILike(v.modelo, $"%{termino}%"))
                    );
                }

                // Búsqueda específica por marca
                if (!string.IsNullOrEmpty(marca))
                {
                    query = query.Where(v => v.marca != null && EF.Functions.ILike(v.marca, marca));
                }

                // Búsqueda específica por modelo
                if (!string.IsNullOrEmpty(modelo))
                {
                    query = query.Where(v => v.modelo != null && EF.Functions.ILike(v.modelo, modelo));
                }

                var resultado = await query
                    .Take(limit)
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
                        v.marco,
                        v.placa,
                        v.version
                    })
                    .ToListAsync();

                _logger.LogInformation("Búsqueda completada. Resultados encontrados: {Count}", resultado.Count);

                return Ok(new
                {
                    success = true,
                    message = "Búsqueda realizada correctamente",
                    count = resultado.Count,
                    limit,
                    filters = new { termino, marca, modelo },
                    data = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar búsqueda de reparaciones");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de reparaciones
        /// </summary>
        /// <returns>Estadísticas generales</returns>
        [HttpGet("estadisticas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de reparaciones");

                var estadisticas = await _context.vCelularesMBP
                    .GroupBy(v => v.marca)
                    .Select(g => new
                    {
                        marca = g.Key,
                        totalModelos = g.Select(v => v.modelo).Distinct().Count(),
                        promedioModulo = g.Average(v => v.arreglomodulo),
                        promedioBateria = g.Average(v => v.arreglobat),
                        promedioPin = g.Average(v => v.arreglopin)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Estadísticas obtenidas correctamente",
                    data = estadisticas,
                    totalMarcas = estadisticas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}
