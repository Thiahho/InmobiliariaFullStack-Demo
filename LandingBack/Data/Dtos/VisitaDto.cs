using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class VisitaCreateDto
    {
        [Required]
        public int PropiedadId { get; set; }
        
        [Required]
        public int AgenteId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ClienteNombre { get; set; } = null!;
        
        [MaxLength(20)]
        public string? ClienteTelefono { get; set; }
        
        [EmailAddress]
        [MaxLength(200)]
        public string? ClienteEmail { get; set; }
        
        [Required]
        public DateTime FechaHora { get; set; }
        
        [Range(30, 480)] // 30 min a 8 horas
        public int DuracionMinutos { get; set; } = 60;
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }

    public class VisitaUpdateDto : VisitaCreateDto
    {
        [Required]
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string? Estado { get; set; }
        
        [MaxLength(1000)]
        public string? NotasVisita { get; set; }
    }

    public class VisitaResponseDto
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string PropiedadCodigo { get; set; } = null!;
        public string PropiedadDireccion { get; set; } = null!;
        public int AgenteId { get; set; }
        public string AgenteNombre { get; set; } = null!;
        public string ClienteNombre { get; set; } = null!;
        public string? ClienteTelefono { get; set; }
        public string? ClienteEmail { get; set; }
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = null!;
        public string? NotasVisita { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class VisitaSearchDto
    {
        public int? AgenteId { get; set; }
        public int? PropiedadId { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? OrderBy { get; set; } = "FechaHora";
        public bool OrderDesc { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DisponibilidadDto
    {
        public int AgenteId { get; set; }
        public DateTime Fecha { get; set; }
        public List<TimeSlot> HorariosDisponibles { get; set; } = new();
    }

    public class TimeSlot
    {
        public TimeSpan Inicio { get; set; }
        public TimeSpan Fin { get; set; }
    }
}