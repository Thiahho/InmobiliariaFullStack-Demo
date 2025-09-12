

namespace DrCell_V02.Data.Dtos
{
    public class VentaDto
    {
        public int Id { get; set; }
        public string PreferenceId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal? CostoTotal { get; set; }
        public decimal? Margen { get; set; }
        public string Estado { get; set; } = "PENDIENTE"; // PENDING, APPROVED, REJECTED, REFUNDED, CANCELLED, PROCESSING
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
        public string? UsuarioId { get; set; }
        public string? Observaciones { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public string? MetodoEnvio { get; set; }
        public string? DireccionEnvio { get; set; }
        public string? NumeroSeguimiento { get; set; }

        // Items vendidos
        public List<VentaItemDto> Items { get; set; } = new List<VentaItemDto>();

        // Propiedades calculadas
        public decimal PorcentajeMargen => MontoTotal > 0 && Margen.HasValue ? (Margen.Value / MontoTotal) * 100 : 0;
        public bool TieneGanancia => Margen.HasValue && Margen.Value > 0;
        
        // Propiedades de auditoría
        public bool FueModificada => FechaModificacion.HasValue;
        public TimeSpan? TiempoDesdeUltimaModificacion => FechaModificacion.HasValue 
            ? DateTime.UtcNow - FechaModificacion.Value 
            : null;
    }

    public class VentaItemDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int VarianteId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}