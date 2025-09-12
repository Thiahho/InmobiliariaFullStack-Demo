using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Controllers.Base;

namespace DrCell_V02.Controllers
{
    [Route("Categorias")]
    [ApiController]
    //[EnableRateLimiting("AuthPolicy")]
    public class CategoriasController : BaseCategoriaController
    {
        public CategoriasController(
            ApplicationDbContext context, 
            IConfiguration config, 
            IProductoService productoService, 
            ICategoriaService categoriaService, 
            ILogger<BaseCategoriaController> logger)
            : base(context, config, productoService, categoriaService, logger)
        {
            // Verificar que las dependencias se resolvieron correctamente
            _logger.LogInformation("CategoriasController inicializado con dependencias: Context={Context}, ProductoService={ProductoService}, CategoriaService={CategoriaService}", 
                context != null, productoService != null, categoriaService != null);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CategoriaDto categoria)
        {
            try
            {
                _logger.LogInformation("Creando nueva categoría: {Nombre}", categoria.Nombre);
                
                var nuevaCategoria = await _categoriaService.AddCategoriaAsync(categoria);
                return CreatedAtAction(nameof(GetCategoriaById), new { id = nuevaCategoria.Id }, nuevaCategoria);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoria");
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] CategoriaDto categoria)
        {
            try
            {
                if (id != categoria.Id)
                {
                    return BadRequest("El ID de la categoria no coincide");
                }

                var categoriaExistente = await _categoriaService.GetCategoriaByIdAsync(id);
                if (categoriaExistente == null)
                {
                    return NotFound("Categoria no encontrada");
                }

                // Actualizar usando el servicio
                await _categoriaService.UpdateCategoriaAsync(categoria);
                
                // Obtener la categoría actualizada
                var categoriaActualizada = await _categoriaService.GetCategoriaByIdAsync(id);
                return Ok(categoriaActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoria");
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        // [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            try
            {
                var categoriaExistente = await _categoriaService.GetCategoriaByIdAsync(id);
                if (categoriaExistente == null)
                {
                    return NotFound("Categoria no encontrada");
                }

                await _categoriaService.DeleteCategoriaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la categoria");
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetCategorias()
        {
            _logger.LogInformation("GetCategorias llamado desde CategoriasController");
            return await base.GetCategorias();
        }

        /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        [AllowAnonymous]
        public override async Task<IActionResult> GetCategoriaById(int id)
        {
            _logger.LogInformation("GetCategoriaById llamado desde CategoriasController para ID: {Id}", id);
            return await base.GetCategoriaById(id);
        }
    }
}