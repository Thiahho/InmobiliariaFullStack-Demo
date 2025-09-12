using Microsoft.AspNetCore.Mvc;

namespace DrCell_V02.Controllers.Services
{
    /// <summary>
    /// Interface para operaciones de solo lectura disponibles para usuarios públicos
    /// </summary>
    public interface IPublicReadOnlyService<T>
    {
        Task<IActionResult> GetAllAsync();
        Task<IActionResult> GetByIdAsync(int id);
        Task<IActionResult> SearchAsync(string termino);
    }

    /// <summary>
    /// Interface para operaciones específicas de categorías públicas
    /// </summary>
    public interface IPublicCategoriaService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetProductosPorCategoriaAsync(int categoriaId);
    }

    /// <summary>
    /// Interface para operaciones específicas de productos públicos
    /// </summary>
    public interface IPublicProductoService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetProductosByCategoriaAsync(int categoriaId);
        Task<IActionResult> GetVariantesAsync(int productoId);
    }

    /// <summary>
    /// Interface para operaciones específicas de celulares públicos
    /// </summary>
    public interface IPublicCelularService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetMarcasAsync();
        Task<IActionResult> GetModelosAsync();
        Task<IActionResult> GetModelosPorMarcaAsync(string marca);
    }

    /// <summary>
    /// Interface para operaciones específicas de módulos públicos
    /// </summary>
    public interface IPublicModuloService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetModulosPorCelularAsync(int celularId);
        Task<IActionResult> GetModulosPorMarcaAsync(string marca);
    }

    /// <summary>
    /// Interface para operaciones específicas de baterías públicas
    /// </summary>
    public interface IPublicBateriaService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetBateriasPorCelularAsync(int celularId);
    }

    /// <summary>
    /// Interface para operaciones específicas de pines públicos
    /// </summary>
    public interface IPublicPinService : IPublicReadOnlyService<object>
    {
        Task<IActionResult> GetPinesPorCelularAsync(int celularId);
        Task<IActionResult> GetPinesPorMarcaAsync(string marca);
    }
}

