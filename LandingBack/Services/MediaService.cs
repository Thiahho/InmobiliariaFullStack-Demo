using AutoMapper;
using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LandingBack.Services
{
    public class MediaService : IMediaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MediaService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageProcessingService _imageProcessing;
        private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff" };
        private readonly string[] _allowedVideoTypes = { ".mp4", ".avi", ".mov", ".wmv" };
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB

        public MediaService(AppDbContext context, IMapper mapper, ILogger<MediaService> logger, 
            IWebHostEnvironment environment, IImageProcessingService imageProcessing)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _environment = environment;
            _imageProcessing = imageProcessing;
        }

        public async Task<IEnumerable<PropiedadMediaDto>> GetMediaByPropiedadIdAsync(int propiedadId)
        {
            try
            {
                var medias = await _context.PropiedadMedias
                    .Where(m => m.PropiedadId == propiedadId)
                    .OrderBy(m => m.Orden)
                    .ThenBy(m => m.Id)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<PropiedadMediaDto>>(medias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener medias de propiedad {PropiedadId}", propiedadId);
                throw new InvalidOperationException($"Error al obtener medias: {ex.Message}", ex);
            }
        }

        public async Task<PropiedadMediaDto> GetMediaByIdAsync(int id)
        {
            try
            {
                var media = await _context.PropiedadMedias
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (media == null)
                    throw new ArgumentException($"No existe media con ID: {id}");

                return _mapper.Map<PropiedadMediaDto>(media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener media {Id}", id);
                throw new InvalidOperationException($"Error al obtener media: {ex.Message}", ex);
            }
        }

        public async Task<PropiedadMediaDto> CreateMediaAsync(int propiedadId, MediaCreateDto mediaCreateDto)
        {
            try
            {
                // Verificar que la propiedad existe
                var propiedadExists = await _context.Propiedades.AnyAsync(p => p.Id == propiedadId);
                if (!propiedadExists)
                    throw new ArgumentException($"No existe propiedad con ID: {propiedadId}");

                // Procesar URL y determinar tipo automáticamente si no se especifica
                var urlProcessed = ProcessExternalUrl(mediaCreateDto.Url);
                var tipoDetectado = DetectMediaType(mediaCreateDto.Url);

                // Obtener próximo orden si no se especifica
                var orden = mediaCreateDto.Orden;
                if (orden == 0)
                {
                    var maxOrden = await _context.PropiedadMedias
                        .Where(m => m.PropiedadId == propiedadId)
                        .MaxAsync(m => (int?)m.Orden) ?? 0;
                    orden = maxOrden + 1;
                }

                var media = new PropiedadMedia
                {
                    PropiedadId = propiedadId,
                    Url = urlProcessed,
                    Titulo = mediaCreateDto.Titulo ?? GenerateDefaultTitle(mediaCreateDto.Url, tipoDetectado),
                    Tipo = string.IsNullOrEmpty(mediaCreateDto.Tipo) ? tipoDetectado : mediaCreateDto.Tipo,
                    Orden = orden
                };

                _context.PropiedadMedias.Add(media);
                await _context.SaveChangesAsync();

                return _mapper.Map<PropiedadMediaDto>(media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear media para propiedad {PropiedadId}", propiedadId);
                throw new InvalidOperationException($"Error al crear media: {ex.Message}", ex);
            }
        }

        public async Task<PropiedadMediaDto> UpdateMediaAsync(int id, MediaUpdateDto mediaUpdateDto)
        {
            try
            {
                var media = await _context.PropiedadMedias.FirstOrDefaultAsync(m => m.Id == id);
                if (media == null)
                    throw new ArgumentException($"No existe media con ID: {id}");

                if (!string.IsNullOrEmpty(mediaUpdateDto.Titulo))
                    media.Titulo = mediaUpdateDto.Titulo;

                if (!string.IsNullOrEmpty(mediaUpdateDto.Tipo))
                    media.Tipo = mediaUpdateDto.Tipo;

                if (mediaUpdateDto.Orden.HasValue)
                    media.Orden = mediaUpdateDto.Orden.Value;

                _context.PropiedadMedias.Update(media);
                await _context.SaveChangesAsync();

                return _mapper.Map<PropiedadMediaDto>(media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar media {Id}", id);
                throw new InvalidOperationException($"Error al actualizar media: {ex.Message}", ex);
            }
        }

        public async Task DeleteMediaAsync(int id)
        {
            try
            {
                var media = await _context.PropiedadMedias.FirstOrDefaultAsync(m => m.Id == id);
                if (media == null)
                    throw new ArgumentException($"No existe media con ID: {id}");

                // Solo eliminar archivo físico si es upload local (no URL externa)
                if (IsLocalFile(media.Url))
                {
                    await DeleteFileAsync(media.Url);
                }

                _context.PropiedadMedias.Remove(media);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar media {Id}", id);
                throw new InvalidOperationException($"Error al eliminar media: {ex.Message}", ex);
            }
        }

        public async Task<PropiedadMediaDto> UploadFileAsync(int propiedadId, IFormFile file, string? titulo = null)
        {
            try
            {
                // Validar archivo
                if (!await ValidateFileAsync(file))
                    throw new ArgumentException("Archivo no válido");

                // Verificar que la propiedad existe
                var propiedadExists = await _context.Propiedades.AnyAsync(p => p.Id == propiedadId);
                if (!propiedadExists)
                    throw new ArgumentException($"No existe propiedad con ID: {propiedadId}");

                string filePath;
                string tipoArchivo = "image";

                // Verificar si es imagen para convertir a WebP
                if (_imageProcessing.IsImageFile(file))
                {
                    // Convertir imagen a WebP optimizada
                    var optimizedBytes = await _imageProcessing.ResizeAndConvertAsync(
                        file, 
                        maxWidth: 1920, 
                        maxHeight: 1080, 
                        quality: 85
                    );

                    // Guardar imagen optimizada
                    filePath = await SaveOptimizedImageAsync(optimizedBytes, file.FileName, "properties");
                    tipoArchivo = "image";
                    
                    _logger.LogInformation("Imagen convertida a WebP: {FileName} -> {FilePath}", file.FileName, filePath);
                }
                else
                {
                    // Para videos y otros archivos, guardar normalmente
                    filePath = await SaveFileAsync(file, "properties");
                    
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    tipoArchivo = _allowedVideoTypes.Contains(extension) ? "video" : "file";
                }

                // Obtener próximo orden
                var maxOrden = await _context.PropiedadMedias
                    .Where(m => m.PropiedadId == propiedadId)
                    .MaxAsync(m => (int?)m.Orden) ?? 0;

                var media = new PropiedadMedia
                {
                    PropiedadId = propiedadId,
                    Url = filePath,
                    Titulo = titulo ?? Path.GetFileNameWithoutExtension(file.FileName),
                    Tipo = tipoArchivo,
                    TipoArchivo = tipoArchivo == "image" ? "webp" : Path.GetExtension(file.FileName).TrimStart('.'),
                    Orden = maxOrden + 1
                };

                _context.PropiedadMedias.Add(media);
                await _context.SaveChangesAsync();

                return _mapper.Map<PropiedadMediaDto>(media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo para propiedad {PropiedadId}", propiedadId);
                throw new InvalidOperationException($"Error al subir archivo: {ex.Message}", ex);
            }
        }

        public async Task ReorderMediaAsync(int propiedadId, List<MediaOrderDto> ordenItems)
        {
            try
            {
                var medias = await _context.PropiedadMedias
                    .Where(m => m.PropiedadId == propiedadId)
                    .ToListAsync();

                foreach (var item in ordenItems)
                {
                    var media = medias.FirstOrDefault(m => m.Id == item.Id);
                    if (media != null)
                    {
                        media.Orden = item.Orden;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reordenar medias de propiedad {PropiedadId}", propiedadId);
                throw new InvalidOperationException($"Error al reordenar medias: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageTypes.Contains(extension) || _allowedVideoTypes.Contains(extension);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder = "properties")
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Retornar ruta relativa para almacenar en BD
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task<string> SaveOptimizedImageAsync(byte[] imageBytes, string originalFileName, string folder = "properties")
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            // Cambiar extensión a .webp
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}.webp";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Retornar ruta relativa para almacenar en BD
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !IsLocalFile(filePath))
                    return;

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar archivo: {FilePath}", filePath);
                // No lanzar excepción, solo log
            }
        }

        #region Métodos para URLs Externas

        /// <summary>
        /// Procesa URLs externas para optimizar su uso (ej: convertir links de Google Drive a URLs directas)
        /// </summary>
        private string ProcessExternalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // Convertir Google Drive share links a direct links
            if (IsGoogleDriveUrl(url))
            {
                return ConvertGoogleDriveUrl(url);
            }

            // Convertir YouTube URLs a embed format
            if (IsYouTubeUrl(url))
            {
                return ConvertYouTubeUrl(url);
            }

            // Para otras URLs, devolver tal como están
            return url;
        }

        /// <summary>
        /// Detecta automáticamente el tipo de media basado en la URL
        /// </summary>
        private string DetectMediaType(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "image";

            url = url.ToLowerInvariant();

            // YouTube, Vimeo, etc.
            if (IsYouTubeUrl(url) || url.Contains("vimeo.com") || url.Contains("player.vimeo"))
                return "video";

            // Tours virtuales
            if (url.Contains("matterport.com") || url.Contains("360") || url.Contains("tour"))
                return "tour";

            // Google Drive - intentar detectar por extensión en el nombre
            if (IsGoogleDriveUrl(url))
            {
                if (url.Contains("video") || url.Contains("mp4") || url.Contains("avi"))
                    return "video";
                return "image"; // Default para Google Drive
            }

            // Por extensión de archivo
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };

            if (imageExtensions.Any(ext => url.Contains(ext)))
                return "image";
            if (videoExtensions.Any(ext => url.Contains(ext)))
                return "video";

            // Default
            return "image";
        }

        private bool IsGoogleDriveUrl(string url)
        {
            return url.Contains("drive.google.com") || url.Contains("docs.google.com");
        }

        private bool IsYouTubeUrl(string url)
        {
            return url.Contains("youtube.com") || url.Contains("youtu.be");
        }

        private bool IsLocalFile(string url)
        {
            return !string.IsNullOrEmpty(url) && 
                   !url.StartsWith("http://") && 
                   !url.StartsWith("https://") &&
                   (url.StartsWith("/") || url.StartsWith("\\"));
        }

        private string ConvertGoogleDriveUrl(string url)
        {
            try
            {
                // Convertir de: https://drive.google.com/file/d/FILE_ID/view?usp=sharing
                // A: https://drive.google.com/uc?export=view&id=FILE_ID
                
                var fileIdMatch = Regex.Match(url, @"/file/d/([a-zA-Z0-9-_]+)");
                if (fileIdMatch.Success)
                {
                    var fileId = fileIdMatch.Groups[1].Value;
                    return $"https://drive.google.com/uc?export=view&id={fileId}";
                }

                // Si ya está en formato directo, devolver tal como está
                if (url.Contains("uc?export=view"))
                    return url;

                return url; // Si no se puede convertir, devolver original
            }
            catch
            {
                return url; // En caso de error, devolver URL original
            }
        }

        private string ConvertYouTubeUrl(string url)
        {
            try
            {
                // Convertir de: https://www.youtube.com/watch?v=VIDEO_ID
                // A: https://www.youtube.com/embed/VIDEO_ID
                
                var videoIdMatch = Regex.Match(url, @"(?:youtube\.com/watch\?v=|youtu\.be/)([a-zA-Z0-9-_]+)");
                if (videoIdMatch.Success)
                {
                    var videoId = videoIdMatch.Groups[1].Value;
                    return $"https://www.youtube.com/embed/{videoId}";
                }

                return url; // Si no se puede convertir, devolver original
            }
            catch
            {
                return url; // En caso de error, devolver URL original
            }
        }

        private string GenerateDefaultTitle(string url, string tipo)
        {
            try
            {
                if (IsYouTubeUrl(url))
                    return "Video de YouTube";
                
                if (IsGoogleDriveUrl(url))
                    return $"Archivo de Google Drive ({tipo})";

                if (url.Contains("matterport"))
                    return "Tour Virtual 3D";

                if (url.Contains("vimeo"))
                    return "Video de Vimeo";

                // Intentar extraer nombre de archivo de la URL
                var uri = new Uri(url);
                var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
                if (!string.IsNullOrEmpty(fileName))
                    return fileName;

                return $"Media {tipo}";
            }
            catch
            {
                return $"Media {tipo}";
            }
        }

        #endregion
    }
}