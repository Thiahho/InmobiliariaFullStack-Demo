using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using DrCell_V02.Controllers.Base;

namespace DrCell_V02.Controllers
{
    [ApiController]
    [Route("Productos")]
    [EnableRateLimiting("AnonPolicy")]
    public class ProductoController : BaseProductoController
    {
        public ProductoController(IProductoService productoService, ILogger<BaseProductoController> logger)
            : base(productoService, logger)
        {
        }

 
        [AllowAnonymous][HttpGet("GetAll")]
        public override async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
        {
                return await base.GetAll();
        }

     
        [AllowAnonymous][HttpGet("GetById/{id}")]
        public override Task<ActionResult<ProductoDto>> GetById(int id)  => base.GetById(id);
        // {
        //     try{
        //         var producto = await base.GetById(id);
        //         return Ok(producto);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"Error al obtener el producto {id}");
        //         return StatusCode(500, "Error interno del servidor");
        //     }
        // }

       /* /// <summary>
        /// Sobrescribe el método base para permitir acceso anónimo
        /// </summary>
        //[AllowAnonymous]
        //public override async Task<ActionResult<IEnumerable<ProductoDto>>> Search([FromQuery] string termino)
        //{
        //    return await base.Search(termino);
        //}
        */


       
        [AllowAnonymous]
        public override Task<ActionResult<IEnumerable<ProductoDto>>> GetByCategoria(int categoriaId) => base.GetByCategoria(categoriaId);
        // {
        //     try{
        //         var productos = await base.GetByCategoria(categoriaId);
        //         return Ok(productos);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"Error al obtener productos por categoría {categoriaId}");
        //         return StatusCode(500, "Error interno del servidor");
        //     }
        // }

        [AllowAnonymous]
        [HttpGet("{id}/variantes")]
        public override Task<ActionResult<IEnumerable<ProductosVariantesDto>>> GetVariantes(int id) => base.GetVariantes(id);
        // {
        //     try{
        //         var variantes = await base.GetVariantes(id);
        //         return Ok(variantes);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"Error al obtener variantes para el producto {id}");
        //         return StatusCode(500, "Error interno del servidor");
        //     }
        // }

       
       

        

      
    }
}
