using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class LeadCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [MaxLength(1000)]
        public string? Mensaje { get; set; }
        
        [Required]
        public int PropiedadId { get; set; }
        
        [MaxLength(20)]
        public string TipoConsulta { get; set; } = "Consulta";
    }

    public class LeadResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Mensaje { get; set; }
        public int PropiedadId { get; set; }
        public string PropiedadCodigo { get; set; } = null!;
        public string PropiedadDireccion { get; set; } = null!;
        public string TipoConsulta { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public int? AgenteAsignadoId { get; set; }
        public string? AgenteAsignadoNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class LeadUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string? Estado { get; set; }
        
        public int? AgenteAsignadoId { get; set; }
        
        [MaxLength(1000)]
        public string? NotasInternas { get; set; }
    }

    public class LeadSearchDto
    {
        public string? Estado { get; set; }
        public int? AgenteAsignadoId { get; set; }
        public int? PropiedadId { get; set; }
        public string? TipoConsulta { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? OrderBy { get; set; } = "FechaCreacion";
        public bool OrderDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}