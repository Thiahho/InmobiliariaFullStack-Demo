using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
namespace DrCell_V02.Controllers
{
    [Route("Modulos")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class ModulosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IModulosService _modulosService;
        private readonly ILogger<ModulosController> _logger;
        
        public ModulosController(ApplicationDbContext context, IModulosService modulosService, ILogger<ModulosController> logger)
        {
            _context = context;
            _modulosService = modulosService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetModulos()
        {
            try
            {
                var modulos = await _modulosService.ObtenerModulosAsync();
                return Ok(new { success = true, data = modulos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos");
                return BadRequest(new { success = false, message = "Error al obtener los módulos", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllModulos()
        {
            try
            {
                var modulos = await _context.Modulos
                    .AsNoTracking()
                    .Select(m => new
                    {
                        m.id,
                        marca = m.marca ?? "",
                        modelo = m.modelo ?? "",
                        arreglo = m.arreglo,
                        costo = m.costo,
                        color = m.color ?? "",
                        marco = m.marco,
                        tipo = m.tipo ?? "",
                        version = m.version ?? ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = modulos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                return BadRequest(new { success = false, message = "Error al obtener los módulos", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("marca/{marca}")]
        public async Task<IActionResult> GetModulosByMarca(string marca)
        {
            try
            {
                var marcas = await _modulosService.ObtenerModulosByMarcaAsync(marca);
                return Ok(new { success = true, data = marcas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos por marca: {Marca}", marca);
                return BadRequest(new { success = false, message = "Error al obtener las marcas de módulos", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("modelo/{modelo}")]
        public async Task<IActionResult> GetModulosByModelo(string modelo)
        {
            try
            {
                var modelos = await _modulosService.ObtenerModulosByModeloAsync(modelo);
                return Ok(new { success = true, data = modelos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos por modelo: {Modelo}", modelo);
                return BadRequest(new { success = false, message = "Error al obtener los módulos por modelo", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("Solo-Modulos/{marca}/{modelo}")]
        public async Task<IActionResult> GetModulosByModelo(string marca, string modelo)
        {
            try
            {
                var modulos = await _modulosService.ObtenerModulosByModeloYMarcaAsync(marca, modelo);
                return Ok(new { success = true, data = modulos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos por marca y modelo: {Marca} {Modelo}", marca, modelo);
                return BadRequest(new { success = false, message = "Error al obtener los módulos por marca y modelo", error = ex.Message });
            }
        }

        [AllowAnonymous]
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

        //[HttpPut("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //public async Task<IActionResult> UpdateModulo(int id, [FromBody] Modulos modulo)
        //{
        //    try
        //    {
        //        if (id != modulo.id)
        //            return BadRequest(new { success = false, message = "El ID del módulo no coincide" });

        //        var existeModulo = await _modulosService.GetModuloByIdAsync(id);
        //        if (existeModulo == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró el módulo con ID {id}" });
        //        }

        //        await _modulosService.UpdateAsync(modulo);
        //        _logger.LogInformation("Módulo {Id} actualizado correctamente", id);
                
        //        return Ok(new { success = true, message = "Módulo actualizado correctamente", data = modulo });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al actualizar el módulo {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> DeleteModulo(int id)
        //{
        //    try
        //    {
        //        var modulo = await _modulosService.GetModuloByIdAsync(id);
        //        if (modulo == null)
        //        {
        //            return NotFound(new { success = false, message = $"No se encontró el módulo con ID {id}" });
        //        }

        //        await _modulosService.DeleteAsync(id);
        //        _logger.LogInformation("Módulo {Id} eliminado correctamente", id);
                
        //        return Ok(new { success = true, message = "Módulo eliminado correctamente" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al eliminar el módulo {Id}", id);
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}

        //[HttpPost("create")]
        //[Authorize(Roles = "ADMIN")]
        //[EnableRateLimiting("CriticalPolicy")]
        //public async Task<IActionResult> CreateModulo([FromBody] Modulos modulo)
        //{
        //    try
        //    {
        //        if(!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var nuevoModulo = await _modulosService.AddAsync(modulo);
        //        return CreatedAtAction(nameof(GetModuloById), new { id = nuevoModulo.id }, new { success = true, data = nuevoModulo });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear el módulo");
        //        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        //    }
        //}
    }
}
