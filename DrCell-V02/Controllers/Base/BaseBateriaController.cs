using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BaseBateriaController : BaseController
    {
        protected readonly IBateriasService _bateriasService;
        protected readonly ILogger<BaseBateriaController> _logger;

        protected BaseBateriaController(
            IBateriasService bateriasService, 
            ILogger<BaseBateriaController> logger)
        {
            _bateriasService = bateriasService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las baterías disponibles
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetBaterias()
        {
            try
            {
                var baterias = await _bateriasService.ObtenerBateriasAsync();
                return Ok(baterias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener baterías");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una batería específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetBateriaById(int id)
        {
            try
            {
                var bateria = await _bateriasService.GetBateriaByIdAsync(id);
                if (bateria == null)
                {
                    return NotFound($"No se encontró la batería con ID {id}");
                }
                return Ok(bateria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener batería con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Busca baterías por término
        ///// </summary>
        //[HttpGet("buscar")]
        //public virtual async Task<IActionResult> BuscarBaterias([FromQuery] string termino)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(termino))
        //        {
        //            return BadRequest("El término de búsqueda no puede estar vacío");
        //        }

        //        var baterias = await _bateriasService.BuscarBateriasAsync(termino);
        //        return Ok(baterias);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al buscar baterías con término: {termino}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene baterías por celular
        /// </summary>
        [HttpGet("celular/{celularId}")]
        public virtual async Task<IActionResult> GetBateriasPorCelular(int celularId)
        {
            try
            {
                var baterias = await _bateriasService.GetBateriaByIdAsync(celularId);
                return Ok(baterias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener baterías para celular {celularId}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
