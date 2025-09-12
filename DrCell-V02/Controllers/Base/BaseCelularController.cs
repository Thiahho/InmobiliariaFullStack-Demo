using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BaseCelularController : BaseController
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IvCelularesInfoService _celularesService;
        protected readonly ILogger<BaseCelularController> _logger;

        protected BaseCelularController(
            ApplicationDbContext context, 
            IvCelularesInfoService celularesService,
            ILogger<BaseCelularController> logger)
        {
            _context = context;
            _celularesService = celularesService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los celulares disponibles
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetCelulares()
        {
            try
            {
                var celulares = await _celularesService.ObtenerCelularesInfoAsync();
                return Ok(celulares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener celulares");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene todas las marcas disponibles
        /// </summary>
        [HttpGet("marcas")]
        public virtual async Task<IActionResult> GetMarcas()
        {
            try
            {
                var marcas = await _celularesService.ObtenerMarcasAsync();
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene todos los modelos disponibles
        /// </summary>
        [HttpGet("modelos")]
        public virtual async Task<IActionResult> GetModelos()
        {
            try
            {
                var modelos = await _celularesService.ObtenerModelosAsync();
                return Ok(modelos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modelos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene modelos por marca específica
        /// </summary>
        [HttpGet("marcas/{marca}/modelos")]
        public virtual async Task<IActionResult> GetModelosPorMarca(string marca)
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
        /// Busca celulares por término de búsqueda
        /// </summary>
        //[HttpGet("buscar")]
        //public virtual async Task<IActionResult> BuscarCelulares([FromQuery] string termino)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(termino))
        //        {
        //            return BadRequest("El término de búsqueda no puede estar vacío");
        //        }

        //        var celulares = await _celularesService.BuscarCelularesAsync(termino);
        //        return Ok(celulares);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al buscar celulares con término: {termino}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene información detallada de un celular específico
        /// </summary>
        //[HttpGet("{id}")]
        //public virtual async Task<IActionResult> GetCelularById(int id)
        //{
        //    try
        //    {
        //        //var celular = await _celularesService.ObtenerCelularPorIdAsync(id);
        //        if (celular == null)
        //        {
        //            return NotFound($"No se encontró el celular con ID {id}");
        //        }
        //        return Ok(celular);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al obtener celular con ID {id}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}
    }
}
