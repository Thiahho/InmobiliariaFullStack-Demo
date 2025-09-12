using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
namespace DrCell_V02.Controllers.Admin
{
    [Route("admin/modulos")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    [Authorize(Roles = "ADMIN")]
    public class AdminModulosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IModulosService _modulosService;
        private readonly ILogger<AdminModulosController> _logger;
        
        public AdminModulosController(ApplicationDbContext context, IModulosService modulosService, ILogger<AdminModulosController> logger)
        {
            _context = context;
            _modulosService = modulosService;
            _logger = logger;
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModulo(int id, [FromBody] Modulos modulo)
        {
            try
            {
                if (id != modulo.id)
                    return BadRequest(new { success = false, message = "El ID del módulo no coincide" });

                var existeModulo = await _modulosService.GetModuloByIdAsync(id);
                if (existeModulo == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el módulo con ID {id}" });
                }

                await _modulosService.UpdateAsync(modulo);
                _logger.LogInformation("Módulo {Id} actualizado correctamente", id);
                
                return Ok(new { success = true, message = "Módulo actualizado correctamente", data = modulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> DeleteModulo(int id)
        {
            try
            {
                var modulo = await _modulosService.GetModuloByIdAsync(id);
                if (modulo == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el módulo con ID {id}" });
                }

                await _modulosService.DeleteAsync(id);
                _logger.LogInformation("Módulo {Id} eliminado correctamente", id);
                
                return Ok(new { success = true, message = "Módulo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost("create")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> CreateModulo([FromBody] Modulos modulo)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var nuevoModulo = await _modulosService.AddAsync(modulo);
                return CreatedAtAction(nameof(GetModuloById), new { id = nuevoModulo.id }, new { success = true, data = nuevoModulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el módulo");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetModuloById(int id)
        {
            try
            {
                var modulo = await _modulosService.GetModuloByIdAsync(id);
                if (modulo == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el módulo con ID {id}" });
                }
                return Ok(new { success = true, data = modulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}
