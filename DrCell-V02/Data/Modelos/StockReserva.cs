using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;

namespace DrCell_V02.Data.Modelos
{
    [Table("stock_reserva")]
    public class StockReserva
    {
        [Key][Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("variante_id")]
        public int VarianteId { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("session_id")]
        [MaxLength(128)]
        public string SessionId { get; set; } = string.Empty;

        [Column("preference_id")]
        [MaxLength(255)]
        public string? PreferenceId { get; set; }

        [Column("fecha_creacion")]
        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_expiracion")]
        [Required]
        public DateTime FechaExpiracion { get; set; }

        [Required]
        [Column("estado")]
        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, CONFIRMADO, LIBERADO, EXPIRADO

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        // Navegación
        public ProductosVariantes Variante { get; set; } = null!;
    }
}