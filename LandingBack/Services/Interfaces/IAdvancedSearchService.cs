using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IAdvancedSearchService
    {
        Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount, SearchStatsDto Stats)> BusquedaAvanzadaAsync(AdvancedSearchDto searchDto);
        Task<IEnumerable<AutocompleteResultDto>> AutocompleteAsync(AutocompleteDto autocompleteDto);
        Task<SearchStatsDto> GetSearchStatsAsync(AdvancedSearchDto searchDto);
        
        // Búsquedas guardadas
        Task<SavedSearchDto> SaveSearchAsync(int usuarioId, string nombre, AdvancedSearchDto searchParams);
        Task<IEnumerable<SavedSearchDto>> GetSavedSearchesAsync(int usuarioId);
        Task DeleteSavedSearchAsync(int searchId, int usuarioId);
        Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount)> ExecuteSavedSearchAsync(int searchId, int usuarioId);
    }
}