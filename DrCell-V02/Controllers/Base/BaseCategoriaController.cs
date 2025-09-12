using Microsoft.AspNetCore.Mvc;
using DrCell_V02.Data;
using DrCell_V02.Services.Interface;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Dtos;

namespace DrCell_V02.Controllers.Base
{
    public abstract class BaseCategoriaController : BaseController
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IConfiguration _configuration;
        protected readonly IProductoService _productoService;
        protected readonly ICategoriaService _categoriaService;
        protected readonly ILogger<BaseCategoriaController> _logger;

        protected BaseCategoriaController(
            ApplicationDbContext context, 
            IConfiguration config, 
            IProductoService productoService, 
            ICategoriaService categoriaService, 
            ILogger<BaseCategoriaController> logger)
        {
            _context = context;
            _configuration = config;
            _productoService = productoService;
            _categoriaService = categoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las categorías disponibles
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categorias = await _categoriaService.GetAllCategoriasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorias");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una categoría específica por ID
        /// </summary>
        [HttpGet("{id:int}")]
        public virtual async Task<IActionResult> GetCategoriaById(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
                if (categoria == null)
                {
                    return NotFound("Categoria no encontrada");
                }
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la categoria por id");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene productos asociados a una categoría
        /// </summary>
        [HttpGet("{id:int}/productos")]
        public virtual async Task<IActionResult> GetProductosPorCategoria(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
                if (categoria == null)
                {
                    return NotFound("Categoria no encontrada");
                }

                var productos = await _productoService.GetByIdWithVarianteAsync(id);
                //var productos = await _productoService.GetProductosByCategoriaAsync(id);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoria");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
