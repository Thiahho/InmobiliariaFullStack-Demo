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
    public class AdminBateriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBateriasService _bateriasService;
        private readonly ILogger<AdminBateriasController> _logger;
        
        public AdminBateriasController(ApplicationDbContext context, IBateriasService bateriasService, ILogger<AdminBateriasController> logger)
        {
            _context = context;
            _bateriasService = bateriasService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBaterias()
        {
            try
            {
                var baterias = await _bateriasService.ObtenerBateriasAsync();
                return Ok(new { success = true, data = baterias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener baterías");
                return BadRequest(new { success = false, message = "Error al obtener las baterías", error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBaterias()
        {
            try
            {
                var baterias = await _context.Baterias
                    .AsNoTracking()
                    .Select(b => new
                    {
                        b.id,
                        marca = b.marca ?? "",
                        modelo = b.modelo ?? "",
                        arreglo = b.arreglo,
                        costo = b.costo,
                        tipo = b.tipo ?? ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = baterias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las baterías");
                return BadRequest(new { success = false, message = "Error al obtener las baterías", error = ex.Message });
            }
        }

        [HttpGet("marca/{marca}")]
        public async Task<IActionResult> GetBateriasByMarca(string marca)
        {
            try
            {
                var marcas = await _bateriasService.ObtenerBateriasByMarcaAsync(marca);
                return Ok(new { success = true, data = marcas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener baterías por marca: {Marca}", marca);
                return BadRequest(new { success = false, message = "Error al obtener las marcas de baterías", error = ex.Message });
            }
        }

        [HttpGet("modelos")]
        public async Task<IActionResult> GetBateriasByModelos()
        {
            try
            {
                var baterias = await _bateriasService.ObtenerBateriasByModeloAsync();
                return Ok(new { success = true, data = baterias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modelos de baterías");
                return BadRequest(new { success = false, message = "Error al obtener los modelos de baterías", error = ex.Message });
            }
        }

        [HttpGet("Solo-Baterias/{marca}/{modelo}")]
        public async Task<IActionResult> GetBateriaByModelo(string marca, string modelo)
        {
            try
            {
                var modulo = await _bateriasService.ObtenerBateriasByModeloYMarcaAsync(marca, modelo);
                return Ok(new { success = true, data = modulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener baterías por marca y modelo: {Marca} {Modelo}", marca, modelo);
                return BadRequest(new { success = false, message = "Error al obtener las baterías por marca y modelo", error = ex.Message });
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

        //[HttpPut("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> UpdateBateria(int id, [FromBody] Baterias bateria)
        //{
        //    try
        //    {
        //        if (id != bateria.id)
        //            return BadRequest(new { success = false, message = "El ID de la batería no coincide" });

        //        var existeBateria = await _bateriasService.GetBateriaByIdAsync(id);
        //        if (existeBateria == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró la batería con ID {id}" });
        //        }

        //        await _bateriasService.UpdateAsync(bateria);
        //        _logger.LogInformation("Batería {Id} actualizada correctamente", id);
                
        //        return Ok(new { success = true, message = "Batería actualizada correctamente", data = bateria });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al actualizar la batería {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> DeleteBateria(int id)
        //{
        //    try
        //    {
        //        var bateria = await _bateriasService.GetBateriaByIdAsync(id);
        //        if (bateria == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró la batería con ID {id}" });
        //        }

        //        await _bateriasService.DeleteAsync(id);
        //        _logger.LogInformation("Batería {Id} eliminada correctamente", id);
                
        //        return Ok(new { success = true, message = "Batería eliminada correctamente" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al eliminar la batería {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpPost("create")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> CreateBateria([FromBody] Baterias bateria)
        //{
        //    try
        //    {
        //        if(!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var nuevaBateria = await _bateriasService.AddAsync(bateria);
        //        return CreatedAtAction(nameof(GetBateriaById), new { id = nuevaBateria.id }, new { success = true, data = nuevaBateria });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear la batería");
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}
    }
}
