using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class Agente
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        
        [Required]
        public string Password { get; set; } = null!;
        
        public bool Activo { get; set; } = true;
        
        public DateTime UltimoLogin { get; set; }
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "Agente"; // Admin|Agente|Cargador
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        public int IntentosFallidosLogin { get; set; } = 0;
        
        public DateTime? BloqueoHasta { get; set; }

        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
