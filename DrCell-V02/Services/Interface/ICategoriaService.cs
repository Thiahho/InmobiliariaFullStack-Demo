using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;

namespace DrCell_V02.Services.Interface
{
    public interface ICategoriaService
    {
        Task<CategoriaDto> GetCategoriaByIdAsync(int id);
        Task<CategoriaDto> AddCategoriaAsync(CategoriaDto categoriaDto);
        Task UpdateCategoriaAsync(CategoriaDto categoriaDto);
        Task DeleteCategoriaAsync(int id);
        Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync();

    }
}
