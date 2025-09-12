namespace DrCell_V02.Data.Dtos
{
    public class FiltroAuditoriaDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Estado { get; set; }
        public string? ModificadoPor { get; set; }
        public bool? SoloModificadas { get; set; } = false;
        public string? UsuarioId { get; set; }
    }

    public class ResumenAuditoriaDto
    {
        public int TotalVentas { get; set; }
        public int VentasModificadas { get; set; }
        public int VentasNoModificadas { get; set; }
        public decimal PorcentajeModificadas { get; set; }
        public string? AdminQueMasModifica { get; set; }
        public int ModificacionesTotales { get; set; }
        public DateTime? UltimaModificacion { get; set; }
    }

    public class ActualizarVentaDto
    {
        public string? Estado { get; set; }
        public string? PaymentId { get; set; }
        public string? Observaciones { get; set; }
        public string? MetodoEnvio { get; set; }
        public string? DireccionEnvio { get; set; }
        public string? NumeroSeguimiento { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public string? MotivoModificacion { get; set; }
    }
}