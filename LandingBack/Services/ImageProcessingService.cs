using LandingBack.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LandingBack.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly string[] _supportedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
        private readonly string[] _supportedMimeTypes = { 
            "image/jpeg", "image/jpg", "image/png", "image/gif", 
            "image/bmp", "image/tiff", "image/webp" 
        };

        public async Task<byte[]> ConvertToWebPAsync(byte[] imageBytes, int quality = 80)
        {
            using var image = Image.Load(imageBytes);
            using var memoryStream = new MemoryStream();
            
            var encoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality
            };

            await image.SaveAsync(memoryStream, encoder);
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ConvertToWebPAsync(IFormFile file, int quality = 80)
        {
            if (!IsImageFile(file))
                throw new ArgumentException("El archivo no es una imagen válida");

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);
            using var memoryStream = new MemoryStream();
            
            var encoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality
            };

            await image.SaveAsync(memoryStream, encoder);
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ResizeAndConvertAsync(byte[] imageBytes, int maxWidth = 1920, int maxHeight = 1080, int quality = 80)
        {
            using var image = Image.Load(imageBytes);
            
            // Calcular nuevas dimensiones manteniendo aspecto
            var (newWidth, newHeight) = CalculateNewDimensions(image.Width, image.Height, maxWidth, maxHeight);
            
            // Redimensionar solo si es necesario
            if (image.Width > newWidth || image.Height > newHeight)
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            using var memoryStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality
            };

            await image.SaveAsync(memoryStream, encoder);
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ResizeAndConvertAsync(IFormFile file, int maxWidth = 1920, int maxHeight = 1080, int quality = 80)
        {
            if (!IsImageFile(file))
                throw new ArgumentException("El archivo no es una imagen válida");

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);
            
            // Calcular nuevas dimensiones manteniendo aspecto
            var (newWidth, newHeight) = CalculateNewDimensions(image.Width, image.Height, maxWidth, maxHeight);
            
            // Redimensionar solo si es necesario
            if (image.Width > newWidth || image.Height > newHeight)
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            using var memoryStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality
            };

            await image.SaveAsync(memoryStream, encoder);
            return memoryStream.ToArray();
        }

        public async Task<(byte[] thumbnail, byte[] optimized)> CreateThumbnailAndOptimizedAsync(IFormFile file, 
            int thumbnailSize = 300, int maxWidth = 1920, int maxHeight = 1080, int quality = 80)
        {
            if (!IsImageFile(file))
                throw new ArgumentException("El archivo no es una imagen válida");

            using var stream = file.OpenReadStream();
            using var originalImage = await Image.LoadAsync(stream);
            
            // Crear thumbnail
            var thumbnailImage = originalImage.Clone(ctx => ctx.Resize(thumbnailSize, thumbnailSize));
            
            using var thumbnailStream = new MemoryStream();
            var thumbnailEncoder = new WebpEncoder
            {
                Quality = 70, // Menor calidad para thumbnail
                Method = WebpEncodingMethod.Fastest
            };
            await thumbnailImage.SaveAsync(thumbnailStream, thumbnailEncoder);
            
            // Crear imagen optimizada
            var (newWidth, newHeight) = CalculateNewDimensions(originalImage.Width, originalImage.Height, maxWidth, maxHeight);
            
            if (originalImage.Width > newWidth || originalImage.Height > newHeight)
            {
                originalImage.Mutate(x => x.Resize(newWidth, newHeight));
            }

            using var optimizedStream = new MemoryStream();
            var optimizedEncoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality
            };
            await originalImage.SaveAsync(optimizedStream, optimizedEncoder);

            return (thumbnailStream.ToArray(), optimizedStream.ToArray());
        }

        public bool IsImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Verificar por extensión
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_supportedImageTypes.Contains(extension))
                return false;

            // Verificar por MIME type
            if (!_supportedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Verificar que realmente sea una imagen intentando cargarla
            try
            {
                using var stream = file.OpenReadStream();
                var imageInfo = Image.Identify(stream);
                return imageInfo != null;
            }
            catch
            {
                return false;
            }
        }

        public bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _supportedImageTypes.Contains(extension);
        }

        public async Task<ImageInfoDto> GetImageInfoAsync(IFormFile file)
        {
            if (!IsImageFile(file))
                throw new ArgumentException("El archivo no es una imagen válida");

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);
            
            return new ImageInfoDto
            {
                Width = image.Width,
                Height = image.Height,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                SizeBytes = file.Length,
                AspectRatio = (double)image.Width / image.Height
            };
        }

        private (int width, int height) CalculateNewDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
                return (originalWidth, originalHeight);

            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }
    }
}