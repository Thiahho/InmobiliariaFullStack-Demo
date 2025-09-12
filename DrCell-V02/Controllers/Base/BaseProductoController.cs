using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Dtos;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BaseProductoController : BaseController
    {
        protected readonly IProductoService _productoService;
        protected readonly ILogger<BaseProductoController> _logger;

        protected BaseProductoController(
            IProductoService productoService, 
            ILogger<BaseProductoController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los productos disponibles
        /// </summary>
        [HttpGet("GetAll")]
        public virtual async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
        {
            try
            {
                var productos = await _productoService.GetAllProductsAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un producto específico por ID con sus variantes
        /// </summary>
        [HttpGet("GetById/{id}")]
        public virtual async Task<ActionResult<ProductoDto>> GetById(int id)
        {
            try
            {
                var producto = await _productoService.GetByIdWithVarianteAsync(id);
                if (producto == null)
                {
                    return NotFound($"No se encontró el producto con ID {id}");
                }
                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el producto {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Busca productos por término de búsqueda
        /// </summary>
        //[HttpGet("buscar")]
        //public virtual async Task<ActionResult<IEnumerable<ProductoDto>>> Search([FromQuery] string termino)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(termino))
        //        {
        //            return BadRequest("El término de búsqueda no puede estar vacío");
        //        }

        //        var productos = await _productoService.SearchProductsAsync(termino);
        //        return Ok(productos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al buscar productos con término: {termino}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        [HttpGet("categoria/{categoriaId}")]
        public virtual async Task<ActionResult<IEnumerable<ProductoDto>>> GetByCategoria(int categoriaId)
        {
            try
            {
                //var productos = await _productoService.GetProductosByCategoriaAsync(categoriaId);
                var productos = await _productoService.GetVariantesByIdAsync(categoriaId);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener productos por categoría {categoriaId}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las variantes de un producto específico
        /// </summary>
        [HttpGet("{id}/variantes")]
        public virtual async Task<ActionResult<IEnumerable<ProductosVariantesDto>>> GetVariantes(int id)
        {
            try
            {
                var variantes = await _productoService.GetVariantesByIdAsync(id);
                return Ok(variantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener variantes del producto {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{productoId}/Ram-Opciones")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctRamAsync(int productoId)
        {
            try
            {
                var producto = await _productoService.GetByIdWithVarianteAsync(productoId);
                if (producto == null)
                {
                    return NotFound($"No se encontró el producto con ID {productoId}");
                }
                var opciones = producto.GetAvailableRAM();
                return Ok(opciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{productoId}/Almacenamiento-Opciones")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctAlmacenamientosAsync(int productoId, [FromQuery] string ram)
        {
            try
            {
                var producto = await _productoService.GetByIdWithVarianteAsync(productoId);
                if (producto == null)
                {
                    return NotFound($"No se encontró el producto con ID {productoId}");
                }
                var almacenamientos = producto.GetAvailableStorage(ram);
                return Ok(almacenamientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{productoId}/Color-Opciones")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctColorsAsync(int productoId, [FromQuery] string ram, [FromQuery] string almacenamiento)
        {
            try
            {
                var producto = await _productoService.GetByIdWithVarianteAsync(productoId);
                if (producto == null)
                {
                    return NotFound($"No se encontró el producto con ID {productoId}");
                }
                var colores = producto.GetAvailableColors(ram, almacenamiento);
                return Ok(colores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{productId}/variante")]
        public async Task<ActionResult<ProductosVariantesDto>> GetVarianteSpecAsync(
            int productId, 
            [FromQuery] string ram, 
            [FromQuery] string storage, 
            [FromQuery] string color)
        {
            try
            {
                var variante = await _productoService.GetVarianteSpecAsync(productId, ram, storage, color);
                if (variante == null)
                {
                    return NotFound($"No se encontró la variante con las especificaciones solicitadas");
                }
                return Ok(variante);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

      


        [HttpGet("variante/{id}")]
        public async Task<ActionResult<ProductosVariantesDto>> GetVarianteById(int id)
        {
            try
            {
                var variante = await _productoService.GetVarianteByIdAsync(id);
                if (variante == null)
                {
                    return NotFound($"No se encontró la variante con ID {id}");
                }
                return Ok(variante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la variante {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
