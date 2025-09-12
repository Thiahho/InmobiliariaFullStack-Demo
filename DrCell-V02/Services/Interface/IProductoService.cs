using System.Threading.Tasks;
using DrCell_V02.Data.Dtos;
using SixLabors.ImageSharp;
namespace DrCell_V02.Services.Interface
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> GetAllVariantesAsync();
        Task<ProductoDto?> GetByIdWithVarianteAsync(int id);
        Task<ProductoDto> AddAsync(ProductoDto productos);
        Task<ProductosVariantesDto> AddVarianteAsync(ProductosVariantesDto productosVariantes);
        //Task UpdateAsync(ProductoDto productos);
        Task ActualizarAsync(ProductoDto productos, CancellationToken ct = default);
        Task DeleteAsync(int id);
        Task<IEnumerable<ProductosVariantesDto>> GetVariantesByIdAsync(int productId);
        Task<ProductosVariantesDto?> GetVarianteSpecAsync(int productId, string ram, string storage, string color);
        Task<ProductosVariantesDto?> GetVarianteByIdAsync(int varianteId);
        Task<IEnumerable<string>> GetDistintAlmacenamientosAsync(string ram, int productId);
        Task<IEnumerable<string>> GetDistintColorAsync(string ram, string almacenamiento, int productId);
        Task<IEnumerable<ProductoDto>> GetAllProductsAsync();
        
        // Nuevos métodos
        Task UpdateVarianteAsync(ProductosVariantesDto variante);
        Task DeleteVarianteAsync(int varianteId);
        Task<bool> ExistsVarianteAsync(int productoId, string ram, string almacenamiento, string color);
        Task<bool> ExistsProductoAsync(string marca, string modelo);

        byte[] toWebpp(byte[] input, int size= 60, int quality= 80);
        bool IsReasonableSize(byte[] input, int maxBytes);
    }
}
