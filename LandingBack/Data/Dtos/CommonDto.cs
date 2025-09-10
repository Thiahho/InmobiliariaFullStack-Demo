using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class AutocompleteDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Count { get; set; }
    }

    public class ContactFormDto
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
        
        [Required]
        [MaxLength(1000)]
        public string Mensaje { get; set; } = null!;
        
        [MaxLength(20)]
        public string? Asunto { get; set; }
        
        public int? PropiedadId { get; set; }
        
        [Required]
        public string CaptchaToken { get; set; } = null!;
    }

    public class BulkUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        
        public bool UpdateExisting { get; set; } = false;
        
        public bool ValidateOnly { get; set; } = false;
    }

    public class BulkUploadResultDto
    {
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int ErrorRows { get; set; }
        public List<BulkUploadError> Errors { get; set; } = new();
        public bool IsValidationOnly { get; set; }
    }

    public class BulkUploadError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string Error { get; set; } = null!;
    }

    public class StatsDto
    {
        public int TotalPropiedades { get; set; }
        public int PropiedadesActivas { get; set; }
        public int PropiedadesVendidas { get; set; }
        public int PropiedadesReservadas { get; set; }
        public int TotalLeads { get; set; }
        public int LeadsNuevos { get; set; }
        public int LeadsEnProceso { get; set; }
        public int LeadsCerrados { get; set; }
        public int VisitasHoy { get; set; }
        public int VisitasEstaSemana { get; set; }
        public decimal ValorTotalCartera { get; set; }
        public decimal PromedioPrecios { get; set; }
    }
}