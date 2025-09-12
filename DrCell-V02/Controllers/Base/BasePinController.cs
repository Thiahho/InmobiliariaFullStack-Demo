using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BasePinController : BaseController
    {
        protected readonly IPinesService _pinesService;
        protected readonly ILogger<BasePinController> _logger;

        protected BasePinController(
            IPinesService pinesService, 
            ILogger<BasePinController> logger)
        {
            _pinesService = pinesService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los pines disponibles
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetPines()
        {
            try
            {
                var pines = await _pinesService.ObtenerPinesAsync();
                return Ok(pines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pines");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un pin específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetPinById(int id)
        {
            try
            {
                var pin = await _pinesService.GetPinByIdAsync(id);
                if (pin == null)
                {
                    return NotFound($"No se encontró el pin con ID {id}");
                }
                return Ok(pin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener pin con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Busca pines por término
        /// </summary>
        //[HttpGet("buscar")]
        //public virtual async Task<IActionResult> BuscarPines([FromQuery] string termino)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(termino))
        //        {
        //            return BadRequest("El término de búsqueda no puede estar vacío");
        //        }

        //        var pines = await _pinesService.BuscarPinesAsync(termino);
        //        return Ok(pines);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al buscar pines con término: {termino}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene pines por celular
        /// </summary>
        [HttpGet("celular/{celularId}")]
        public virtual async Task<IActionResult> GetPinesPorCelular(int celularId)
        {
            try
            {
                var pines = await _pinesService.GetPinByIdAsync(celularId);
                return Ok(pines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener pines para celular {celularId}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
