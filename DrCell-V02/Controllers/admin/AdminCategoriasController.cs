using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Controllers.Base;

namespace DrCell_V02.Controllers.Admin
{
    [Route("admin/categorias")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    //[EnableRateLimiting("AuthPolicy")]
    public class AdminCategoriasController : BaseCategoriaController
    {
        public AdminCategoriasController(ApplicationDbContext context, IConfiguration config, IProductoService productoService, ICategoriaService categoriaService, ILogger<BaseCategoriaController> logger)
            : base(context, config, productoService, categoriaService, logger)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CategoriaDto categoria)
        {
            try
            {
                var nuevaCategoria = await _categoriaService.AddCategoriaAsync(categoria);
                return CreatedAtAction(nameof(GetCategoriaById), new { id = nuevaCategoria.Id }, nuevaCategoria);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoria");
                return StatusCode(500, "Error interno del servidor");
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

                categoriaExistente.Nombre = categoria.Nombre;
                await _categoriaService.UpdateCategoriaAsync(categoriaExistente);
                return Ok(categoriaExistente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoria");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id:int}")]
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
                return StatusCode(500, "Error interno del servidor");
            }

        }

        // Los métodos GET se heredan automáticamente de BaseCategoriaController
        // y ya tienen autorización ADMIN aplicada a nivel de controlador

    }
}
