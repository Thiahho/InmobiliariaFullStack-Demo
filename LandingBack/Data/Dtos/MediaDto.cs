using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class MediaUploadDto
    {
        [Required]
        public int PropiedadId { get; set; }
        
        [Required]
        public IFormFile File { get; set; } = null!;
        
        [MaxLength(100)]
        public string? Titulo { get; set; }
        
        [Range(0, 999)]
        public int Orden { get; set; } = 0;
    }

    public class MediaUrlDto
    {
        [Required]
        public int PropiedadId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = null!; // Foto|Video|VR|Plano
        
        [Required]
        [Url]
        [MaxLength(500)]
        public string Url { get; set; } = null!;
        
        [MaxLength(100)]
        public string? Titulo { get; set; }
        
        [Range(0, 999)]
        public int Orden { get; set; } = 0;
    }

    public class MediaResponseDto
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string Tipo { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? Titulo { get; set; }
        public int Orden { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    public class MediaUpdateOrderDto
    {
        [Required]
        public List<MediaOrderItem> Items { get; set; } = new();
    }

    public class MediaOrderItem
    {
        [Required]
        public int Id { get; set; }
        
        [Range(0, 999)]
        public int Orden { get; set; }
    }

    public class PresignedUrlRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FileName { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = null!;
        
        [Required]
        public int PropiedadId { get; set; }
        
        [MaxLength(100)]
        public string? Titulo { get; set; }
    }

    public class PresignedUrlResponseDto
    {
        public string UploadUrl { get; set; } = null!;
        public string ViewUrl { get; set; } = null!;
        public string Key { get; set; } = null!;
        public Dictionary<string, string> Fields { get; set; } = new();
    }

    public class BulkMediaUploadDto
    {
        [Required]
        public int PropiedadId { get; set; }
        
        [Required]
        public List<IFormFile> Files { get; set; } = new();
        
        public string? TituloBase { get; set; }
    }
}