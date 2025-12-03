using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IMediaService
    {
        Task<IEnumerable<PropiedadMediaDto>> GetMediaByPropiedadIdAsync(int propiedadId);
        Task<PropiedadMediaDto> GetMediaByIdAsync(int id);
        Task<(byte[] Data, string ContentType, string FileName)?> GetMediaBinaryDataAsync(int id);
        Task<PropiedadMediaDto> CreateMediaAsync(int propiedadId, MediaCreateDto mediaCreateDto);
        Task<PropiedadMediaDto> UpdateMediaAsync(int id, MediaUpdateDto mediaUpdateDto);
        Task DeleteMediaAsync(int id);
        Task<PropiedadMediaDto> UploadFileAsync(int propiedadId, IFormFile file, string? titulo = null);
        Task ReorderMediaAsync(int propiedadId, List<MediaOrderDto> ordenItems);
        Task<bool> ValidateFileAsync(IFormFile file);
        Task<string> SaveFileAsync(IFormFile file, string folder = "properties");
        Task DeleteFileAsync(string filePath);
    }

    // DTOs adicionales para compatibilidad
    public class MediaCreateDto
    {
        public string Url { get; set; } = null!;
        public string? Titulo { get; set; }
        public string Tipo { get; set; } = "image";
        public int Orden { get; set; } = 0;
    }

    public class MediaUpdateDto
    {
        public string? Titulo { get; set; }
        public string? Tipo { get; set; }
        public int? Orden { get; set; }
    }

    public class MediaOrderDto
    {
        public int Id { get; set; }
        public int Orden { get; set; }
    }
}