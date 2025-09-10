using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LandingBack.Data.Dtos
{
    public class PropiedadCreateDto
    {
        [Required]
        [MaxLength(20)]
        public string Codigo { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = null!;
        
        [Required]
        [MaxLength(10)]
        public string Operacion { get; set; } = "Venta";
        
        [Required]
        [MaxLength(100)]
        public string Barrio { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Comuna { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Direccion { get; set; } = null!;
        
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        
        [MaxLength(3)]
        public string Moneda { get; set; } = "USD";
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? Expensas { get; set; }
        
        [Required]
        [Range(1, 20)]
        public int Ambientes { get; set; }
        
        [Range(0, 20)]
        public int? Dormitorios { get; set; }
        
        [Range(0, 10)]
        public int? Banos { get; set; }
        
        public bool? Cochera { get; set; }
        
        [Range(0, 10000)]
        public int? MetrosCubiertos { get; set; }
        
        [Range(0, 10000)]
        public int? MetrosTotales { get; set; }
        
        [Range(0, 200)]
        public int? Antiguedad { get; set; }
        
        [Range(-10, 100)]
        public int? Piso { get; set; }
        
        public bool? AptoCredito { get; set; }
        
        public Dictionary<string, object>? Amenities { get; set; }
        
        [MaxLength(20)]
        public string Estado { get; set; } = "Activa";
        
        public bool Destacado { get; set; } = false;
    }

    public class PropiedadUpdateDto : PropiedadCreateDto
    {
        public int Id { get; set; }
    }

    public class PropiedadResponseDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Operacion { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string Comuna { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string Moneda { get; set; } = null!;
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
        public Dictionary<string, object>? Amenities { get; set; }
        public string Estado { get; set; } = null!;
        public bool Destacado { get; set; }
        public DateTime FechaPublicacionUtc { get; set; }
        public List<PropiedadMediaDto> Medias { get; set; } = new();
    }

    public class PropiedadSearchDto
    {
        public string? Operacion { get; set; }
        public string? Tipo { get; set; }
        public string? Barrio { get; set; }
        public string? Comuna { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public int? Ambientes { get; set; }
        public int? Dormitorios { get; set; }
        public bool? Cochera { get; set; }
        public string? Estado { get; set; }
        public bool? Destacado { get; set; }
        public string? OrderBy { get; set; } = "FechaPublicacionUtc";
        public bool OrderDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PropiedadMediaDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? Titulo { get; set; }
        public int Orden { get; set; }
    }
}