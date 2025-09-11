using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Modelos
{
    public class SavedSearch
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        
        [Required]
        public string ParametrosBusqueda { get; set; } = null!; // JSON serializado
        
        public bool NotificacionesActivas { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime? UltimaEjecucion { get; set; }
        
        public int? ResultadosUltimaEjecucion { get; set; }

        // Navegación
        public Agente Usuario { get; set; } = null!;
    }
}