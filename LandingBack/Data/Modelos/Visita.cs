using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class Visita
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public int AgenteId { get; set; }
        public int? LeadId { get; set; }
        public DateTime InicioUtc { get; set; }
        public DateTime FinUtc { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente|Confirmada|Realizada|Cancelada
        public string? Notas { get; set; }

        public Propiedad Propiedad { get; set; } = null!;
        public Agente Agente { get; set; } = null!;
        public Lead? Lead { get; set; }
    }
}
