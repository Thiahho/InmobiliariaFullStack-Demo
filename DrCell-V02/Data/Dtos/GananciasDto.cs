namespace DrCell_V02.Data.Dtos
{
    public class ReporteGananciasDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalCostos { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal MargenPromedio { get; set; }
        public int ProductosVendidos { get; set; }
        public int VentasConGanancia { get; set; }
        public int VentasSinGanancia { get; set; }
        public decimal PorcentajeRentabilidad { get; set; }
        public ProductoRentabilidadDto? ProductoMasRentable { get; set; }
        public ProductoRentabilidadDto? ProductoMenosRentable { get; set; }
    }

    public class ProductoRentabilidadDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string VarianteDescripcion { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalCostos { get; set; }
        public decimal GananciaTotal { get; set; }
        public decimal MargenPromedio { get; set; }
        public decimal PorcentajeMargen { get; set; }
        public decimal GananciaPorUnidad { get; set; }
    }

    public class ComparativaGananciasDto
    {
        public ReporteGananciasDto PeriodoActual { get; set; } = new();
        public ReporteGananciasDto PeriodoAnterior { get; set; } = new();
        public decimal CrecimientoVentas { get; set; }
        public decimal CrecimientoGanancias { get; set; }
        public decimal CambioMargenPromedio { get; set; }
        public TendenciaEnum Tendencia { get; set; }
        public List<string> Alertas { get; set; } = new();
    }

    public class RentabilidadPorCategoriaDto
    {
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public decimal TotalVendido { get; set; }
        public decimal TotalCostos { get; set; }
        public decimal GananciaTotal { get; set; }
        public decimal MargenPromedio { get; set; }
        public int ProductosVendidos { get; set; }
        public int VariantesActivas { get; set; }
        public ProductoRentabilidadDto? MejorProducto { get; set; }
        public ProductoRentabilidadDto? PeorProducto { get; set; }
    }

    public class AlertaRentabilidadDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string VarianteDescripcion { get; set; } = string.Empty;
        public decimal MargenActual { get; set; }
        public decimal MargenMinimo { get; set; }
        public TipoAlertaEnum TipoAlerta { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaAlerta { get; set; } = DateTime.UtcNow;
    }

    public class FiltroGananciasDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? MargenMinimo { get; set; }
        public decimal? MargenMaximo { get; set; }
        public bool? SoloProductosRentables { get; set; }
        public OrdenamientoGanancias Ordenamiento { get; set; } = OrdenamientoGanancias.GananciaDesc;
    }

    public class AnalisisMargenDto
    {
        public int VarianteId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public decimal PrecioVentaActual { get; set; }
        public decimal CostoActual { get; set; }
        public decimal MargenActual { get; set; }
        public decimal PorcentajeMargenActual { get; set; }
        public decimal MargenOptimo { get; set; }
        public decimal PrecioOptimoSugerido { get; set; }
        public decimal ImpactoEnGanancias { get; set; }
        public List<EscenarioPrecioDto> Escenarios { get; set; } = new();
    }

    public class EscenarioPrecioDto
    {
        public decimal PrecioSugerido { get; set; }
        public decimal Margen { get; set; }
        public decimal PorcentajeMargen { get; set; }
        public decimal ImpactoEstimado { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public enum TendenciaEnum
    {
        Mejorando,
        Estable,
        Empeorando
    }

    public enum TipoAlertaEnum
    {
        MargenBajo,
        SinGanancia,
        CostoAlto,
        ProductoNoRentable,
        VentasBajas
    }

    public enum OrdenamientoGanancias
    {
        GananciaDesc,
        GananciaAsc,
        MargenDesc,
        MargenAsc,
        VentasDesc,
        VentasAsc
    }
}