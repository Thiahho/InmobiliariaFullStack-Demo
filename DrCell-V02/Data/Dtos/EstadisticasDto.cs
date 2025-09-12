namespace DrCell_V02.Data.Dtos
{
    public class EstadisticasPeriodoDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalVentas { get; set; }
        public int TotalProductosVendidos { get; set; }
        public decimal MontoTotalVendido { get; set; }
        public decimal? CostoTotalVendido { get; set; }
        public decimal? MargenTotal { get; set; }
        public decimal? PorcentajeMargen { get; set; }
        public decimal TicketPromedio { get; set; }
        public int VentasAprobadas { get; set; }
        public int VentasPendientes { get; set; }
        public int VentasRechazadas { get; set; }
        public decimal PorcentajeAprobacion { get; set; }
    }

    public class TopProductoDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string VarianteDescripcion { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PrecioPromedio { get; set; }
        public int NumeroVentas { get; set; }
    }

    public class ResumenDiarioDto
    {
        public DateTime Fecha { get; set; }
        public int VentasDelDia { get; set; }
        public decimal MontoDelDia { get; set; }
        public decimal? CostoDelDia { get; set; }
        public decimal? MargenDelDia { get; set; }
        public int ProductosVendidos { get; set; }
        public decimal TicketPromedio { get; set; }
    }

    public class VentasPorEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ComparativaPeriodoDto
    {
        public EstadisticasPeriodoDto PeriodoActual { get; set; } = new();
        public EstadisticasPeriodoDto PeriodoAnterior { get; set; } = new();
        public decimal CrecimientoVentas { get; set; }
        public decimal CrecimientoMonto { get; set; }
        public decimal CrecimientoMargen { get; set; }
    }

    public enum TipoPeriodo
    {
        Hoy,
        Semana,
        Mes,
        Trimestre,
        Año,
        Personalizado
    }

    public class FiltroEstadisticasDto
    {
        public TipoPeriodo TipoPeriodo { get; set; } = TipoPeriodo.Mes;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Estado { get; set; }
        public int? CategoriaId { get; set; }
    }
}