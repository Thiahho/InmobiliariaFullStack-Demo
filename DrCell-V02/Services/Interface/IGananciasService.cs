using DrCell_V02.Data.Dtos;

namespace DrCell_V02.Services.Interface
{
    public interface IGananciasService
    {
        Task<ReporteGananciasDto> GenerarReporteGananciasAsync(FiltroGananciasDto filtro);
        Task<IEnumerable<ProductoRentabilidadDto>> GetProductosMasRentablesAsync(int cantidad = 10, FiltroGananciasDto? filtro = null);
        Task<IEnumerable<ProductoRentabilidadDto>> GetProductosMenosRentablesAsync(int cantidad = 10, FiltroGananciasDto? filtro = null);
        Task<IEnumerable<RentabilidadPorCategoriaDto>> GetRentabilidadPorCategoriaAsync(FiltroGananciasDto? filtro = null);
        Task<ComparativaGananciasDto> GetComparativaGananciasAsync(TipoPeriodo periodo);
        Task<IEnumerable<AlertaRentabilidadDto>> GetAlertasRentabilidadAsync(decimal margenMinimo = 20);
        Task<AnalisisMargenDto> AnalisisMargenProductoAsync(int varianteId);
        Task ActualizarCostosProductoAsync(int varianteId, decimal nuevoCosto, string usuario);
        Task<bool> RecalcularMargenesVentasAsync(DateTime? fechaInicio = null);
    }
}