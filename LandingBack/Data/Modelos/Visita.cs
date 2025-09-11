using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class Visita
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public int AgenteId { get; set; }
        public int? LeadId { get; set; }
        
        // Información del cliente
        public string ClienteNombre { get; set; } = null!;
        public string? ClienteTelefono { get; set; }
        public string? ClienteEmail { get; set; }
        
        // Fecha y hora
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; } = 60;
        
        // Estado y notas
        public string Estado { get; set; } = "Pendiente"; // Pendiente|Confirmada|Realizada|Cancelada
        public string? Observaciones { get; set; }
        public string? NotasVisita { get; set; }
        
        // Metadatos
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public int? CreadoPorUsuarioId { get; set; }

        // Navegación
        public Propiedad Propiedad { get; set; } = null!;
        public Agente Agente { get; set; } = null!;
        public Lead? Lead { get; set; }
    }
}
