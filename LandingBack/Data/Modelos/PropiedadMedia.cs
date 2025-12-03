using System.ComponentModel.DataAnnotations.Schema;

namespace LandingBack.Data.Modelos
{
    public class PropiedadMedia
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string Url { get; set; } = null!; // Mantener para URLs externas (YouTube, etc.)
        public byte[]? DatosArchivo { get; set; } // Datos binarios de la imagen/archivo
        public string? NombreArchivo { get; set; } // Nombre original del archivo
        public string? ContentType { get; set; } // Tipo MIME (image/jpeg, image/png, etc.)
        public long? TamanoBytes { get; set; } // Tamaño del archivo en bytes
        public string? Titulo { get; set; }
        public string Tipo { get; set; } = "image"; // image|video|plano|tour
        public string TipoArchivo { get; set; } = "jpg"; // jpg|png|webp|mp4|pdf
        public int Orden { get; set; } = 0;
        public bool EsPrincipal { get; set; } = false;
        public Propiedad Propiedad { get; set; } = null!;
    }
}
