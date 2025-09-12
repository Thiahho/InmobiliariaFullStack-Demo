namespace DrCell_V02.Data.Dtos
{
    public class CostoProductoDto
    {
        public int Id { get; set; }
        public int VarianteId { get; set; }
        public decimal CostoCompra { get; set; }
        public DateTime FechaVigencia { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin { get; set; }
        public bool EsVigente { get; set; } = true;
        public string? Proveedor { get; set; }
        public string? Observaciones { get; set; }
        public string CreadoPor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class CrearCostoProductoDto
    {
        public int VarianteId { get; set; }
        public decimal CostoCompra { get; set; }
        public DateTime FechaVigencia { get; set; } = DateTime.UtcNow;
        public string? Proveedor { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ActualizarCostoProductoDto
    {
        public decimal CostoCompra { get; set; }
        public string? Proveedor { get; set; }
        public string? Observaciones { get; set; }
    }
}