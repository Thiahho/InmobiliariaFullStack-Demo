using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Modelos
{
    public class VisitaAuditLog
    {
        public int Id { get; set; }
        
        [Required]
        public int VisitaId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = null!; // Crear|Modificar|Confirmar|Cancelar|Reagendar|Realizada
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string UsuarioNombre { get; set; } = null!;
        
        public string? ValoresAnteriores { get; set; } // JSON con valores previos
        public string? ValoresNuevos { get; set; } // JSON con valores nuevos
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }
        
        [Required]
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Navegación
        public Visita Visita { get; set; } = null!;
    }
}