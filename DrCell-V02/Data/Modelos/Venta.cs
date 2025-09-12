using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)][Column("preference_id")]
        public string PreferenceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)][Column("payment_id")]
        public string PaymentId { get; set; } = string.Empty;

        [Required][Column("monto_total")]
        public decimal MontoTotal { get; set; }

        [Column("costo_total")]
        public decimal? CostoTotal { get; set; }

        [Column("margen")]
        public decimal? Margen { get; set; }

        [Required]
        [MaxLength(20)][Column("estado")]
        public string Estado { get; set; } = "PENDIENTE"; // PENDING, APPROVED, REJECTED, REFUNDED, CANCELLED, PROCESSING

        [Required][Column("fecha_venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [MaxLength(500)][Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [MaxLength(255)][Column("modificado_por")]
        public string? ModificadoPor { get; set; }

        [Column("metodo_envio")]
        [MaxLength(50)]
        public string? MetodoEnvio { get; set; }

        [Column("direccion_envio")]
        [MaxLength(500)]
        public string? DireccionEnvio { get; set; }

        [Column("numero_seguimiento")]
        [MaxLength(100)]
        public string? NumeroSeguimiento { get; set; }

        // Items vendidos
        public ICollection<VentaItem> Items { get; set; } = new List<VentaItem>();

        // Navegación al usuario temporalmente comentada para evitar problemas de FK
        // public Usuario? Usuario { get; set; }
    }


}