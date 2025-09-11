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

        [MaxLength(20)]
        public string Canal { get; set; } = "Web";

        [MaxLength(100)]
        public string Origen { get; set; } = "site";

        // Campos automáticos (se llenan en el controller)
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
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

    public class LeadAssignDto
    {
        [Required]
        public int LeadId { get; set; }
        
        [Required]
        public int AgenteId { get; set; }
        
        [MaxLength(500)]
        public string? Notas { get; set; }
    }

    public class LeadStatusUpdateDto
    {
        [Required]
        public int LeadId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = null!;
        
        [MaxLength(1000)]
        public string? NotasInternas { get; set; }
    }

    public class LeadStatsDto
    {
        public int TotalLeads { get; set; }
        public int LeadsNuevos { get; set; }
        public int LeadsEnProceso { get; set; }
        public int LeadsCerrados { get; set; }
        public int LeadsSinAsignar { get; set; }
        public double TasaConversion { get; set; }
        public Dictionary<string, int> LeadsPorCanal { get; set; } = new();
        public Dictionary<string, int> LeadsPorOrigen { get; set; } = new();
        public List<LeadsByDateDto> LeadsPorFecha { get; set; } = new();
    }

    public class LeadsByDateDto
    {
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
    }

    public class BulkLeadActionDto
    {
        [Required]
        public List<int> LeadIds { get; set; } = new();
        
        [Required]
        [MaxLength(20)]
        public string Accion { get; set; } = null!; // Asignar|CambiarEstado|Eliminar
        
        public int? AgenteId { get; set; }
        public string? Estado { get; set; }
        public string? Notas { get; set; }
    }
}