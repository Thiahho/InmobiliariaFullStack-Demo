using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
namespace DrCell_V02.Controllers
{
    [Route("Baterias")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    [Authorize(Roles = "ADMIN")]
    public class AdminAdminBateriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBateriasService _bateriasService;
        private readonly ILogger<AdminBateriasController> _logger;
        
        public AdminAdminBateriasController(ApplicationDbContext context, IBateriasService bateriasService, ILogger<AdminBateriasController> logger)
        {
            _context = context;
            _bateriasService = bateriasService;
            _logger = logger;
        }
       

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateBateria(int id, [FromBody] Baterias bateria)
        {
            try
            {
                if (id != bateria.id)
                    return BadRequest(new { success = false, message = "El ID de la batería no coincide" });

                var existeBateria = await _bateriasService.GetBateriaByIdAsync(id);
                if (existeBateria == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró la batería con ID {id}" });
                }

                await _bateriasService.UpdateAsync(bateria);
                _logger.LogInformation("Batería {Id} actualizada correctamente", id);
                
                return Ok(new { success = true, message = "Batería actualizada correctamente", data = bateria });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la batería {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> DeleteBateria(int id)
        {
            try
            {
                var bateria = await _bateriasService.GetBateriaByIdAsync(id);
                if (bateria == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró la batería con ID {id}" });
                }

                await _bateriasService.DeleteAsync(id);
                _logger.LogInformation("Batería {Id} eliminada correctamente", id);
                
                return Ok(new { success = true, message = "Batería eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la batería {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "ADMIN")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> CreateBateria([FromBody] Baterias bateria)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var nuevaBateria = await _bateriasService.AddAsync(bateria);
                return CreatedAtAction(nameof(GetBateriaById), new { id = nuevaBateria.id }, new { success = true, data = nuevaBateria });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la batería");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBateriaById(int id)
        {
            try
            {
                var bateria = await _bateriasService.GetBateriaByIdAsync(id);
                if (bateria == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró la batería con ID {id}" });
                }
                return Ok(new { success = true, data = bateria });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la batería {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

    }
}
