using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Modelos
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Accion { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string Entidad { get; set; } = null!;
        
        public int? EntidadId { get; set; }
        
        public string? ValorAnterior { get; set; }
        
        public string? ValorNuevo { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        public int? AgenteId { get; set; }
        public Agente? Agente { get; set; }
    }
}