using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using DrCell_V02.Controllers.Base;
using SixLabors.ImageSharp.PixelFormats;

namespace DrCell_V02.Controllers.Admin
{
    [ApiController]
    [Route("admin/productos")]
    [Authorize(Roles = "ADMIN")]
    [EnableRateLimiting("AuthPolicy")]
    public class AdminProductoController : BaseProductoController
    {
        public AdminProductoController(IProductoService productoService, ILogger<BaseProductoController> logger)
            : base(productoService, logger)
        {
        }

        /*  // ========================================
        // MÉTODOS DE LECTURA - HEREDADOS DE BASE + ESPECÍFICOS ADMIN
        // ========================================
        // Los métodos GET básicos se heredan automáticamente de BaseProductoController
        // y ya tienen autorización ADMIN aplicada a nivel de controlador
        */

        /// <summary>
        /// Obtener una variante específica por ID - Solo ADMIN
        /// </summary>
        /// 
     
        /// <summary>
        /// Obtener todas las variantes con información completa - Solo ADMIN
        /// </summary>
       

    
        [HttpPost]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] ProductoDto producto)
        {
            try
            {
                _logger.LogInformation("Creando producto: {@Producto}", producto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var nuevoProducto = await _productoService.AddAsync(producto);
                _logger.LogInformation("Producto creado exitosamente con ID: {Id}", nuevoProducto.Id);
                return CreatedAtAction(nameof(GetById), new { id = nuevoProducto.Id }, nuevoProducto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear el producto: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de operación al crear el producto: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear el producto");
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductoDto productoDto)
        {
            try
            {
                _logger.LogInformation("Actualizando producto {Id}: {@Producto}", id, productoDto);

                if (id != productoDto.Id)
                {
                    _logger.LogWarning("ID del producto no coincide: {Id} != {DtoId}", id, productoDto.Id);
                    return BadRequest("El ID del producto no coincide");
                }

                // Obtener el producto existente
                var existingProducto = await _productoService.GetByIdWithVarianteAsync(id);
                if (existingProducto == null)
                {
                    _logger.LogWarning("No se encontró el producto con ID {Id}", id);
                    return NotFound($"No se encontró el producto con ID {id}");
                }

                _logger.LogInformation("Producto existente encontrado: {@ExistingProducto}", existingProducto);

                // Si no se proporciona una nueva imagen, mantener la existente
                if (string.IsNullOrEmpty(productoDto.Img))
                {
                    _logger.LogInformation("Manteniendo imagen existente del producto {Id}", id);
                    productoDto.Img = existingProducto.Img;
                }

                _logger.LogInformation("Llamando a ActualizarAsync para producto {Id}", id);
                await _productoService.ActualizarAsync(productoDto);
                
                _logger.LogInformation("Producto {Id} actualizado exitosamente", id);
                return Ok(productoDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar el producto {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Producto {Id} no encontrado: {Message}", id, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el producto {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("CriticalPolicy")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var producto = await _productoService.GetByIdWithVarianteAsync(id);
                if (producto == null)
                {
                    return NotFound($"No se encontró el producto con ID {id}");
                }

                await _productoService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar el producto {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

    

        [HttpPost("variante")]
        public async Task<IActionResult> CreateVariante([FromBody] ProductosVariantesDto varianteDto)
        {
            try
            {
                // Verificar si ya existe una variante con las mismas especificaciones
                var existingVariante = await _productoService.GetVarianteSpecAsync(
                    varianteDto.ProductoId,
                    varianteDto.Ram,
                    varianteDto.Almacenamiento,
                    varianteDto.Color
                );

                if (existingVariante != null)
                {
                    return BadRequest("Ya existe una variante con estas especificaciones");
                }

                // Crear la nueva variante
                var createdVariante = await _productoService.AddVarianteAsync(varianteDto);
                return CreatedAtAction(nameof(GetVarianteById), new { id = createdVariante.Id }, createdVariante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la variante");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("variante/{varianteId}")]
        public async Task<IActionResult> UpdateVariante(int varianteId, [FromBody] ProductosVariantesDto varianteDto)
        {
            // 1. Buscar la variante existente
            var existingVariante = await _productoService.GetVarianteByIdAsync(varianteId);
            if (existingVariante == null)
                return NotFound($"No se encontró la variante con ID {varianteId}");

            // 2. Validar duplicados (misma combinación de ram, almacenamiento y color en el mismo producto)
            var duplicateCheck = await _productoService.GetVarianteSpecAsync(
                existingVariante.ProductoId,
                varianteDto.Ram,
                varianteDto.Almacenamiento,
                varianteDto.Color
            );
            if (duplicateCheck != null && duplicateCheck.Id != varianteId)
                return BadRequest("Ya existe una variante con estas especificaciones");

            // 3. Asignar los IDs correctos al DTO
            varianteDto.Id = varianteId;
            varianteDto.ProductoId = existingVariante.ProductoId;

            // 4. Actualizar la variante (solo update, nunca delete)
            await _productoService.UpdateVarianteAsync(varianteDto);

            // 5. Devolver la variante actualizada
            return Ok(varianteDto);
        }

        [HttpDelete("variante/{id}")]
        public async Task<IActionResult> DeleteVariante(int id)
        {
            try
            {
                var variante = await _productoService.GetVarianteByIdAsync(id);
                if (variante == null)
                {
                    return NotFound($"No se encontró la variante con ID {id}");
                }

                await _productoService.DeleteVarianteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la variante {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

       
    }
}
