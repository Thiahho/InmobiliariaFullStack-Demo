using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;

namespace DrCell_V02.Services.Interface
{
    public interface IVentaService
    {
        Task<VentaDto> GetVentaByIdAsync(int id);
        Task<IEnumerable<VentaDto>> GetAllVentasAsync();

        // Estadísticas y reportes
        Task<EstadisticasPeriodoDto> GetEstadisticasPeriodoAsync(FiltroEstadisticasDto filtro);
        Task<IEnumerable<TopProductoDto>> GetTopProductosAsync(int cantidad = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<IEnumerable<ResumenDiarioDto>> GetResumenDiarioAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<VentasPorEstadoDto>> GetVentasPorEstadoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<ComparativaPeriodoDto> GetComparativaPeriodoAsync(TipoPeriodo tipoPeriodo);

        // Auditoría
        Task<IEnumerable<VentaDto>> GetVentasConAuditoriaAsync(FiltroAuditoriaDto filtro);
        Task<ResumenAuditoriaDto> GetResumenAuditoriaAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<VentaDto?> ActualizarVentaAsync(int ventaId, ActualizarVentaDto actualizacion);
    }
}