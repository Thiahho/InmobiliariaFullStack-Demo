using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json;

namespace LandingBack.Data.Modelos
{
    public class Propiedad
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Tipo { get; set; } = null!;        // Departamento|Casa|PH|...
        public string Operacion { get; set; } = "Venta"; // Venta|Alquiler
        public string Barrio { get; set; } = null!;
        public string Comuna { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public Point Geo { get; set; }           // SRID 4326
        public string Moneda { get; set; } = "USD";      // USD|ARS
        public decimal Precio { get; set; }
        public decimal? Expensas { get; set; }
        public int Ambientes { get; set; }
        public int? Dormitorios { get; set; }
        public int? Banos { get; set; }
        public bool? Cochera { get; set; }
        public int? MetrosCubiertos { get; set; }
        public int? MetrosTotales { get; set; }
        public int? Antiguedad { get; set; }
        public int? Piso { get; set; }
        public bool? AptoCredito { get; set; }
        public JsonDocument? Amenities { get; set; }     // jsonb
        public string Estado { get; set; } = "Activa";   // Activa|Reservada|Vendida|Pausada
        public bool Destacado { get; set; } = false;
        public DateTime FechaPublicacionUtc { get; set; } = DateTime.UtcNow;

        public ICollection<PropiedadMedia> Medias { get; set; } = new List<PropiedadMedia>();
        public ICollection<PropiedadHistorial> Historial { get; set; } = new List<PropiedadHistorial>();
    }
}
