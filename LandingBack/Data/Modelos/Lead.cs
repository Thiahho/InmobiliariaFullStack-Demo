using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json;

namespace LandingBack.Data.Modelos
{
    public class Lead
    {
        public int Id { get; set; }
        public int? PropiedadId { get; set; }
        public int? AgenteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Canal { get; set; } = "Web";    // Web|Whatsapp|Llamada|Email|Portal|Otro
        public string Origen { get; set; } = "site";  // utm/campaña
        public string Mensaje { get; set; } = null!;
        public string Estado { get; set; } = "Nuevo"; // Nuevo|EnProceso|NoContesta|Cerrado
        public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

        public Propiedad? Propiedad { get; set; }
        public Agente? Agente { get; set; }
    }
}
