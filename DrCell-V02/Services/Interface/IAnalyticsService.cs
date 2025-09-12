using DrCell_V02.Data.Dtos;

namespace DrCell_V02.Services.Interface
{
    public interface IAnalyticsService
    {
        // Dashboard y KPIs
        Task<DashboardKpiDto> GenerarDashboardKpisAsync();
        Task<MetricasGeneralesDto> GetMetricasGeneralesAsync();
        Task<List<KpiDto>> CalcularKpisPrincipalesAsync();
        Task<List<TendenciaDto>> AnalizarTendenciasAsync(int diasAnalisis = 30);

        // Proyecciones y predicciones
        Task<ProyeccionVentasDto> ProyectarVentasAsync(int diasProyeccion = 30);
        Task<List<EscenarioProyeccionDto>> GenerarEscenariosProyeccionAsync(int diasProyeccion = 30);

        // Análisis de clientes
        Task<AnalisisClientesDto> AnalisisClientesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<SegmentoClienteDto>> SegmentarClientesAsync();
        Task<List<ComportamientoClienteDto>> AnalisisPatronesCompraAsync();

        // Análisis de inventario
        Task<AnalisisInventarioDto> AnalisisInventarioAsync();
        Task<List<ProductoRotacionDto>> ProductosLentaRotacionAsync(int cantidad = 10);
        Task<List<AlertaInventarioDto>> AlertasInventarioAsync();

        // Análisis temporal y estacionalidad
        Task<List<MetricasTemporalesDto>> AnalisisTemporalAsync(DateTime fechaInicio, DateTime fechaFin, string agrupacion = "dia");
        Task<AnalisisEstacionalidadDto> AnalisisEstacionalidadAsync(int mesesAnalisis = 12);

        // Alertas inteligentes
        Task<List<AlertaDto>> GenerarAlertasInteligentesAsync();
        Task<List<AlertaDto>> AlertasTendenciasNegativasAsync();
    }
}