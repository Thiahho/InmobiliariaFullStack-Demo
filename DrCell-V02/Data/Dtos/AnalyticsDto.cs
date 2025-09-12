namespace DrCell_V02.Data.Dtos
{
    public class DashboardKpiDto
    {
        public MetricasGeneralesDto MetricasGenerales { get; set; } = new();
        public List<KpiDto> KpisPrincipales { get; set; } = new();
        public List<TendenciaDto> Tendencias { get; set; } = new();
        public List<AlertaDto> Alertas { get; set; } = new();
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    }

    public class MetricasGeneralesDto
    {
        public decimal VentasHoy { get; set; }
        public decimal VentasSemana { get; set; }
        public decimal VentasMes { get; set; }
        public decimal GananciasHoy { get; set; }
        public decimal GananciasSemana { get; set; }
        public decimal GananciasMes { get; set; }
        public int ProductosVendidosHoy { get; set; }
        public int ProductosVendidosSemana { get; set; }
        public int ProductosVendidosMes { get; set; }
        public int VentasPendientes { get; set; }
        public decimal TicketPromedioMes { get; set; }
    }

    public class KpiDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal ValorAnterior { get; set; }
        public decimal CambioPorcentual { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public TipoTendenciaKpi Tendencia { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool EsCritico { get; set; }
    }

    public class TendenciaDto
    {
        public string Metrica { get; set; } = string.Empty;
        public List<PuntoTendenciaDto> Datos { get; set; } = new();
        public decimal TasaCrecimiento { get; set; }
        public TipoTendenciaEnum Direccion { get; set; }
        public int DiasAnalizados { get; set; }
    }

    public class PuntoTendenciaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
    }

    public class AlertaDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public TipoAlertaEnum Tipo { get; set; }
        public NivelAlertaEnum Nivel { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Accion { get; set; } = string.Empty;
        public Dictionary<string, object> Datos { get; set; } = new();
    }

    public class ProyeccionVentasDto
    {
        public DateTime FechaProyeccion { get; set; }
        public decimal VentasProyectadas { get; set; }
        public decimal GananciasProyectadas { get; set; }
        public decimal MargenConfianza { get; set; }
        public List<EscenarioProyeccionDto> Escenarios { get; set; } = new();
        public string MetodoProyeccion { get; set; } = string.Empty;
        public List<FactorInfluenciaDto> FactoresInfluencia { get; set; } = new();
    }

    public class EscenarioProyeccionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Probabilidad { get; set; }
        public decimal VentasProyectadas { get; set; }
        public decimal GananciasProyectadas { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class FactorInfluenciaDto
    {
        public string Factor { get; set; } = string.Empty;
        public decimal Impacto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class AnalisisClientesDto
    {
        public int TotalClientes { get; set; }
        public int ClientesNuevos { get; set; }
        public int ClientesRecurrentes { get; set; }
        public decimal TasaRetencion { get; set; }
        public decimal ValorVidaPromedio { get; set; }
        public List<SegmentoClienteDto> Segmentos { get; set; } = new();
        public List<ComportamientoClienteDto> PatronesCompra { get; set; } = new();
    }

    public class SegmentoClienteDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PorcentajeTotal { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal FrecuenciaCompra { get; set; }
        public string Caracteristicas { get; set; } = string.Empty;
    }

    public class ComportamientoClienteDto
    {
        public string Patron { get; set; } = string.Empty;
        public int Frecuencia { get; set; }
        public decimal ValorPromedio { get; set; }
        public List<string> ProductosPreferidos { get; set; } = new();
        public string HorarioPreferido { get; set; } = string.Empty;
    }

    public class AnalisisInventarioDto
    {
        public int ProductosTotales { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorInventario { get; set; }
        public decimal RotacionPromedio { get; set; }
        public List<ProductoRotacionDto> ProductosLentaRotacion { get; set; } = new();
        public List<ProductoRotacionDto> ProductosAltaRotacion { get; set; } = new();
        public List<AlertaInventarioDto> Alertas { get; set; } = new();
    }

    public class ProductoRotacionDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int DiasSinVentas { get; set; }
        public int CantidadVendida { get; set; }
        public int VelocidadRotacion { get; set; }
        public decimal RotacionAnual { get; set; }
        public int DiasPromedioPermanencia { get; set; }
        public decimal ValorInventario { get; set; }
        public string Recomendacion { get; set; } = string.Empty;
    }

    public class AlertaInventarioDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public TipoAlertaInventario Tipo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int DiasStock { get; set; }
    }

    public class MetricasTemporalesDto
    {
        public DateTime Fecha { get; set; }
        public decimal Ventas { get; set; }
        public decimal Ganancias { get; set; }
        public int Transacciones { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal MargenPromedio { get; set; }
    }

    public class AnalisisEstacionalidadDto
    {
        public List<EstacionalidadMesDto> EstacionalidadMensual { get; set; } = new();
        public List<EstacionalidadDiaDto> EstacionalidadSemanal { get; set; } = new();
        public List<EstacionalidadHoraDto> EstacionalidadHoraria { get; set; } = new();
        public string TendenciaGeneral { get; set; } = string.Empty;
        public string MesMasVentas { get; set; } = string.Empty;
        public string DiaMasVentas { get; set; } = string.Empty;
        public string HoraMasVentas { get; set; } = string.Empty;
        public int MesesAnalizados { get; set; }
        public decimal PromedioVentasMensual { get; set; }
        public List<PatronEstacionalDto> PatronesEstacionales { get; set; } = new();
        public string MesMayorVenta { get; set; } = string.Empty;
        public string MesMenorVenta { get; set; } = string.Empty;
        public decimal VariacionEstacional { get; set; }
    }

    public class EstacionalidadMesDto
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal VentasPromedio { get; set; }
        public decimal IndiceEstacionalidad { get; set; }
    }

    public class PatronEstacionalDto
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal Ventas { get; set; }
        public decimal IndiceEstacionalidad { get; set; }
        public int NumeroTransacciones { get; set; }
    }

    public class EstacionalidadDiaDto
    {
        public DayOfWeek Dia { get; set; }
        public string NombreDia { get; set; } = string.Empty;
        public decimal VentasPromedio { get; set; }
        public decimal IndiceEstacionalidad { get; set; }
    }

    public class EstacionalidadHoraDto
    {
        public int Hora { get; set; }
        public decimal VentasPromedio { get; set; }
        public decimal IndiceEstacionalidad { get; set; }
    }

    public class FiltroAnalyticsDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? CategoriaId { get; set; }
        public string? MetricaPrincipal { get; set; }
        public TipoAnalisis TipoAnalisis { get; set; } = TipoAnalisis.General;
        public int? DiasProyeccion { get; set; } = 30;
        public bool IncluirPredicciones { get; set; } = false;
    }

    public enum TipoTendenciaKpi
    {
        Subiendo,
        Bajando,
        Estable,
        Volatil
    }

    public enum TipoTendenciaEnum
    {
        Creciente,
        Decreciente,
        Estable,
        Ciclica
    }

    public enum NivelAlertaEnum
    {
        Informacion,
        Advertencia,
        Critica,
        Urgente
    }

    public enum TipoAlertaInventario
    {
        StockBajo,
        SinStock,
        SobreStock,
        BajaRotacion
    }

    public enum TipoAnalisis
    {
        General,
        Ventas,
        Ganancias,
        Inventario,
        Clientes,
        Estacionalidad
    }
}