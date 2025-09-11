using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IPropiedadesService
    {
        Task<PropiedadResponseDto> GetPropiedadByIdAsync(int id);
        Task<IEnumerable<PropiedadResponseDto>> GetAllPropiedadesAsync();
        Task<IEnumerable<PropiedadResponseDto>> GetPropiedadesByFiltroAsync(string? ubicacion = null, decimal? precioMin = null, decimal? precioMax = null, string? tipo = null);
        Task<PropiedadCreateDto> CreatePropiedadAsync(PropiedadCreateDto propiedadResponseDto);
        Task UpdatePropiedadAsync(PropiedadUpdateDto propiedades, int? usuarioId = null);
        Task DeletePropiedadAsync(int id);
        Task<bool> ExistePropiedadAsync(int id, string codigo, string barrio, string comuna);
        Task<int> GetTotalPropiedadesCountAsync();
        Task<IEnumerable<PropiedadResponseDto>> GetPropiedadesPaginadasAsync(int pagina, int tamanoPagina);
        Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount)> SearchPropiedadesAsync(PropiedadSearchDto searchDto);
    }
}
