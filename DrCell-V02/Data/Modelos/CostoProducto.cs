using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Data.Modelos
{
    [Table("costo_productos")]
    public class CostoProducto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("variante_id")]
        public int VarianteId { get; set; }

        [Required]
        [Column("costo_compra")]
        public decimal CostoCompra { get; set; }

        [Required]
        [Column("fecha_vigencia")]
        public DateTime FechaVigencia { get; set; } = DateTime.UtcNow;

        [Column("fecha_fin")]
        public DateTime? FechaFin { get; set; }

        [Required]
        [Column("es_vigente")]
        public bool EsVigente { get; set; } = true;

        [MaxLength(255)]
        [Column("proveedor")]
        public string? Proveedor { get; set; }

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Required]
        [Column("creado_por")]
        [MaxLength(255)]
        public string CreadoPor { get; set; } = string.Empty;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("modificado_por")]
        [MaxLength(255)]
        public string? ModificadoPor { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        // Navegación
        public ProductosVariantes Variante { get; set; } = null!;
    }
}