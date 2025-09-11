using System.ComponentModel.DataAnnotations.Schema;
// using System.Drawing; // Comentado temporalmente
// using System.Text.Json;

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
        // Coordenadas geográficas como campos separados para Entity Framework
        public double? GeoLatitud { get; set; }
        public double? GeoLongitud { get; set; }
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
        public string? AmenitiesJson { get; set; }     // Para almacenar JSON como string en la DB
        public string Estado { get; set; } = "Activo";   // Activa|Reservada|Vendida|Pausada
        public bool Destacado { get; set; } = false;
        public DateTime FechaPublicacionUtc { get; set; } = DateTime.UtcNow;
        
        // Propiedades adicionales para búsqueda avanzada
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string? Orientacion { get; set; }
        public bool DisponibilidadInmediata { get; set; } = true;
        public bool AceptaMascotas { get; set; } = false;
        public bool AceptaCredito { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        // Aliases para compatibilidad con búsquedas
        public string TipoPropiedad => Tipo;
        public string TipoOperacion => Operacion;
        public int? Baños => Banos;
        public int? SuperficieCubierta => MetrosCubiertos;
        public int? SuperficieTotal => MetrosTotales;

        // Navegación
        public ICollection<PropiedadMedia> Medias { get; set; } = new List<PropiedadMedia>();
        public ICollection<PropiedadMedia> PropiedadMedias => Medias; // Alias para compatibilidad
        public ICollection<PropiedadHistorial> Historial { get; set; } = new List<PropiedadHistorial>();
    }
}
