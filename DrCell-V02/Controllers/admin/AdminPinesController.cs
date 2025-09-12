using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DrCell_V02.Controllers.Admin
{
    [Route("admin/pines")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    [EnableRateLimiting("AuthPolicy")]
    public class AdminPinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPinesService _pinesService;
        private readonly ILogger<AdminPinesController> _logger;
        
        public AdminPinesController(ApplicationDbContext context, IPinesService pinesService, ILogger<AdminPinesController> logger)
        {
            _context = context;
            _pinesService = pinesService;
            _logger = logger;
        }

     
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePin(int id, [FromBody] Pines pin)
        {
            try
            {
                if (id != pin.id)
                    return BadRequest(new { success = false, message = "El ID del pin no coincide" });

                var existePin = await _pinesService.GetPinByIdAsync(id);
                if (existePin == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el pin con ID {id}" });
                }

                await _pinesService.UpdateAsync(pin);
                _logger.LogInformation("Pin {Id} actualizado correctamente", id);
                
                return Ok(new { success = true, message = "Pin actualizado correctamente", data = pin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el pin {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> DeletePin(int id)
        {
            try
            {
                var pin = await _pinesService.GetPinByIdAsync(id);
                if (pin == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el pin con ID {id}" });
                }

                await _pinesService.DeleteAsync(id);
                _logger.LogInformation("Pin {Id} eliminado correctamente", id);
                
                return Ok(new { success = true, message = "Pin eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el pin {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost("create")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> CreatePin([FromBody] Pines pin)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var nuevoPin = await _pinesService.AddAsync(pin);
                return CreatedAtAction(nameof(GetPinById), new { id = nuevoPin.id }, new { success = true, data = nuevoPin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el pin");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPinById(int id)
        {
            try
            {
                var pin = await _pinesService.GetPinByIdAsync(id);
                if (pin == null)
                {
                    return NotFound(new { success = false, message = $"No se encontró el pin con ID {id}" });
                }
                return Ok(new { success = true, data = pin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el pin {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}
