using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json;

namespace LandingBack.Data.Modelos
{
    public class Lead
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public int? AgenteAsignadoId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Mensaje { get; set; }
        public string Canal { get; set; } = "Web";    // Web|Whatsapp|Llamada|Email|Portal|Otro
        public string Origen { get; set; } = "site";  // utm/campaña/referrer
        public string TipoConsulta { get; set; } = "Consulta"; // Consulta|Visita|Informacion|Otro
        public string Estado { get; set; } = "Nuevo"; // Nuevo|EnProceso|NoContesta|Cerrado
        public string? NotasInternas { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public Propiedad Propiedad { get; set; } = null!;
        public Agente? AgenteAsignado { get; set; }
    }
}
