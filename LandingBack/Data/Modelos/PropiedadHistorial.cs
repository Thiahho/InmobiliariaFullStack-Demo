using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class PropiedadHistorial
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string Campo { get; set; } = null!;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public Guid? UsuarioId { get; set; }
        public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
        public Propiedad Propiedad { get; set; } = null!;
    }
}
