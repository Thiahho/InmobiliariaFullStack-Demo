using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BaseModuloController : BaseController
    {
        protected readonly IModulosService _modulosService;
        protected readonly ILogger<BaseModuloController> _logger;

        protected BaseModuloController(
            IModulosService modulosService, 
            ILogger<BaseModuloController> logger)
        {
            _modulosService = modulosService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los módulos disponibles
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetModulos()
        {
            try
            {
                var modulos = await _modulosService.ObtenerModulosAsync();
                return Ok(modulos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un módulo específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetModuloById(int id)
        {
            try
            {
                var modulo = await _modulosService.GetModuloByIdAsync(id);
                if (modulo == null)
                {
                    return NotFound($"No se encontró el módulo con ID {id}");
                }
                return Ok(modulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener módulo con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Busca módulos por término
        /// </summary>
        //[HttpGet("buscar")]
        //public virtual async Task<IActionResult> BuscarModulos([FromQuery] string termino)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(termino))
        //        {
        //            return BadRequest("El término de búsqueda no puede estar vacío");
        //        }

        //        var modulos = await _modulosService.BuscarModulosAsync(termino);
        //        return Ok(modulos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al buscar módulos con término: {termino}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene módulos por celular
        /// </summary>
        [HttpGet("celular/{celularId}")]
        public virtual async Task<IActionResult> GetModulosPorCelular(int celularId)
        {
            try
            {
                var modulos = await _modulosService.GetModuloByIdAsync(celularId);
                return Ok(modulos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener módulos para celular {celularId}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
