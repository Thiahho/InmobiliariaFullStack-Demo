namespace LandingBack.Services.Interfaces
{
    public interface IImageProcessingService
    {
        Task<byte[]> ConvertToWebPAsync(byte[] imageBytes, int quality = 80);
        Task<byte[]> ConvertToWebPAsync(IFormFile file, int quality = 80);
        Task<byte[]> ResizeAndConvertAsync(byte[] imageBytes, int maxWidth = 1920, int maxHeight = 1080, int quality = 80);
        Task<byte[]> ResizeAndConvertAsync(IFormFile file, int maxWidth = 1920, int maxHeight = 1080, int quality = 80);
        Task<(byte[] thumbnail, byte[] optimized)> CreateThumbnailAndOptimizedAsync(IFormFile file, 
            int thumbnailSize = 300, int maxWidth = 1920, int maxHeight = 1080, int quality = 80);
        bool IsImageFile(IFormFile file);
        bool IsImageFile(string fileName);
        Task<ImageInfoDto> GetImageInfoAsync(IFormFile file);
    }

    public class ImageInfoDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = null!;
        public long SizeBytes { get; set; }
        public double AspectRatio { get; set; }
    }
}