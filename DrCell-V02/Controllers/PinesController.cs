using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DrCell_V02.Controllers
{
    [Route("Pines")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class PinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPinesService _pinesService;
        private readonly ILogger<PinesController> _logger;
        
        public PinesController(ApplicationDbContext context, IPinesService pinesService, ILogger<PinesController> logger)
        {
            _context = context;
            _pinesService = pinesService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPines()
        {
            try
            {
                var pines = await _pinesService.ObtenerPinesAsync();
                return Ok(new { success = true, data = pines });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pines");
                return BadRequest(new { success = false, message = "Error al obtener los pines", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPines()
        {
            try
            {
                var pines = await _context.Pines
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.id,
                        marca = p.marca ?? "",
                        modelo = p.modelo ?? "",
                        arreglo = p.arreglo,
                        costo = p.costo,
                        placa = p.placa,
                        tipo = p.tipo ?? ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = pines });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pines");
                return BadRequest(new { success = false, message = "Error al obtener los pines", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("marca/{marca}")]
        public async Task<IActionResult> GetPinesByMarca(string marca)
        {
            try
            {
                var marcas = await _pinesService.ObtenerPinesByMarcaAsync(marca);
                return Ok(new { success = true, data = marcas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pines por marca: {Marca}", marca);
                return BadRequest(new { success = false, message = "Error al obtener las marcas de pines", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("modelos")]
        public async Task<IActionResult> GetPinesByModelos()
        {
            try
            {
                var pines = await _pinesService.ObtenerPinesByModeloAsync();
                return Ok(new { success = true, data = pines });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modelos de pines");
                return BadRequest(new { success = false, message = "Error al obtener los modelos de pines", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("Solo-Pines/{marca}/{modelo}")]
        public async Task<IActionResult> GetPinesByModelo(string marca, string modelo)
        {
            try
            {
                var pines = await _pinesService.ObtenerPinesByModeloYMarcaAsync(marca, modelo);
                return Ok(new { success = true, data = pines });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pines por marca y modelo: {Marca} {Modelo}", marca, modelo);
                return BadRequest(new { success = false, message = "Error al obtener los pines por marca y modelo", error = ex.Message });
            }
        }

        [AllowAnonymous]
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

        //[HttpPut("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> UpdatePin(int id, [FromBody] Pines pin)
        //{
        //    try
        //    {
        //        if (id != pin.id)
        //            return BadRequest(new { success = false, message = "El ID del pin no coincide" });

        //        var existePin = await _pinesService.GetPinByIdAsync(id);
        //        if (existePin == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró el pin con ID {id}" });
        //        }

        //        await _pinesService.UpdateAsync(pin);
        //        _logger.LogInformation("Pin {Id} actualizado correctamente", id);
                
        //        return Ok(new { success = true, message = "Pin actualizado correctamente", data = pin });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al actualizar el pin {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> DeletePin(int id)
        //{
        //    try
        //    {
        //        var pin = await _pinesService.GetPinByIdAsync(id);
        //        if (pin == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró el pin con ID {id}" });
        //        }

        //        await _pinesService.DeleteAsync(id);
        //        _logger.LogInformation("Pin {Id} eliminado correctamente", id);
                
        //        return Ok(new { success = true, message = "Pin eliminado correctamente" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al eliminar el pin {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpPost("create")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> CreatePin([FromBody] Pines pin)
        //{
        //    try
        //    {
        //        if(!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var nuevoPin = await _pinesService.AddAsync(pin);
        //        return CreatedAtAction(nameof(GetPinById), new { id = nuevoPin.id }, new { success = true, data = nuevoPin });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear el pin");
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}
    }
}
