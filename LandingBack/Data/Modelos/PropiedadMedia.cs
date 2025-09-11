using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class PropiedadMedia
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string Url { get; set; } = null!;
        public string? Titulo { get; set; }
        public string Tipo { get; set; } = "image"; // image|video|plano|tour
        public string TipoArchivo { get; set; } = "jpg"; // jpg|png|webp|mp4|pdf
        public int Orden { get; set; } = 0;
        public bool EsPrincipal { get; set; } = false;
        public Propiedad Propiedad { get; set; } = null!;
    }
}
