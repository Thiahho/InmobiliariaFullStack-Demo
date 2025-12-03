using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly IImageProcessingService _imageProcessing;
        private readonly ILogger<MediaController> _logger;

        public MediaController(IMediaService mediaService, IImageProcessingService imageProcessing, ILogger<MediaController> logger)
        {
            _mediaService = mediaService;
            _imageProcessing = imageProcessing;
            _logger = logger;
        }

        // GET: api/media/propiedad/{propiedadId}
        [HttpGet("propiedad/{propiedadId}")]
        public async Task<ActionResult<IEnumerable<PropiedadMediaDto>>> GetMediaByPropiedad(int propiedadId)
        {
            try
            {
                var medias = await _mediaService.GetMediaByPropiedadIdAsync(propiedadId);
                return Ok(medias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener medias de propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/media/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PropiedadMediaDto>> GetMedia(int id)
        {
            try
            {
                var media = await _mediaService.GetMediaByIdAsync(id);
                return Ok(media);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener media {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/media/{id}/image - Endpoint para servir la imagen binaria
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetMediaImage(int id)
        {
            try
            {
                var result = await _mediaService.GetMediaBinaryDataAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("No se encontró imagen para media {Id}", id);
                    return NotFound("Imagen no encontrada");
                }

                var (data, contentType, fileName) = result.Value;

                // Retornar la imagen con cache headers para mejorar rendimiento
                Response.Headers.Add("Cache-Control", "public, max-age=31536000"); // Cache por 1 año
                Response.Headers.Add("ETag", $"\"{id}\"");

                return File(data, contentType, fileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de media {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/media/upload/{propiedadId}
        [HttpPost("upload/{propiedadId}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<PropiedadMediaDto>> UploadFile(int propiedadId, [FromForm] MediaUploadDto uploadDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (uploadDto.File == null)
                    return BadRequest("Archivo requerido");

                var media = await _mediaService.UploadFileAsync(propiedadId, uploadDto.File, uploadDto.Titulo);
                return CreatedAtAction(nameof(GetMedia), new { id = media.Id }, media);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo para propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/media/url/{propiedadId}
        [HttpPost("url/{propiedadId}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<PropiedadMediaDto>> AddExternalUrl(int propiedadId, [FromBody] MediaUrlDto urlDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var mediaCreateDto = new MediaCreateDto
                {
                    Url = urlDto.Url,
                    Titulo = urlDto.Titulo,
                    Tipo = urlDto.Tipo,
                    Orden = urlDto.Orden
                };

                var media = await _mediaService.CreateMediaAsync(propiedadId, mediaCreateDto);
                return CreatedAtAction(nameof(GetMedia), new { id = media.Id }, media);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar URL externa para propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/media/bulk-upload/{propiedadId}
        [HttpPost("bulk-upload/{propiedadId}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<IEnumerable<PropiedadMediaDto>>> BulkUpload(int propiedadId, [FromForm] BulkMediaUploadDto bulkUploadDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (bulkUploadDto.Files == null || !bulkUploadDto.Files.Any())
                    return BadRequest("Al menos un archivo es requerido");

                var results = new List<PropiedadMediaDto>();
                var index = 0;

                foreach (var file in bulkUploadDto.Files)
                {
                    try
                    {
                        var titulo = string.IsNullOrEmpty(bulkUploadDto.TituloBase) 
                            ? null 
                            : $"{bulkUploadDto.TituloBase} {index + 1}";

                        var media = await _mediaService.UploadFileAsync(propiedadId, file, titulo);
                        results.Add(media);
                        index++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al subir archivo {FileName}", file.FileName);
                        // Continuar con los demás archivos
                    }
                }

                if (!results.Any())
                    return BadRequest("No se pudo subir ningún archivo");

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en subida masiva para propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/media/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<IActionResult> UpdateMedia(int id, [FromBody] MediaUpdateDto mediaUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _mediaService.UpdateMediaAsync(id, mediaUpdateDto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar media {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/media/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            try
            {
                await _mediaService.DeleteMediaAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar media {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/media/reorder/{propiedadId}
        [HttpPut("reorder/{propiedadId}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<IActionResult> ReorderMedia(int propiedadId, [FromBody] MediaUpdateOrderDto orderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ordenItems = orderDto.Items.Select(item => new MediaOrderDto
                {
                    Id = item.Id,
                    Orden = item.Orden
                }).ToList();

                await _mediaService.ReorderMediaAsync(propiedadId, ordenItems);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reordenar medias de propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/media/validate-url
        [HttpGet("validate-url")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<object>> ValidateUrl([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return BadRequest("URL requerida");

                // Validaciones básicas
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return Ok(new { valid = false, message = "URL no válida" });

                // Detectar tipo de media
                var tipo = DetectMediaTypeFromUrl(url);
                var isSupported = IsSupportedExternalUrl(url);

                return Ok(new 
                { 
                    valid = isSupported,
                    tipo = tipo,
                    message = isSupported ? "URL válida" : "Tipo de URL no soportada",
                    url = url,
                    processedUrl = ProcessUrlForPreview(url)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar URL {Url}", url);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/media/image-info
        [HttpPost("image-info")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<ImageInfoDto>> GetImageInfo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Archivo requerido");

                if (!_imageProcessing.IsImageFile(file))
                    return BadRequest("El archivo debe ser una imagen válida");

                var imageInfo = await _imageProcessing.GetImageInfoAsync(file);
                
                // Calcular tamaño estimado después de conversión
                var estimatedWebPSize = (long)(imageInfo.SizeBytes * 0.25); // WebP reduce ~75%
                
                return Ok(new
                {
                    original = imageInfo,
                    webpEstimation = new
                    {
                        format = "webp",
                        estimatedSizeBytes = estimatedWebPSize,
                        estimatedSizeMB = Math.Round(estimatedWebPSize / 1024.0 / 1024.0, 2),
                        compressionRatio = Math.Round((1 - (double)estimatedWebPSize / imageInfo.SizeBytes) * 100, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de imagen");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/media/convert-preview
        [HttpPost("convert-preview")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult> PreviewConversion(IFormFile file, [FromQuery] int quality = 85)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Archivo requerido");

                if (!_imageProcessing.IsImageFile(file))
                    return BadRequest("El archivo debe ser una imagen válida");

                // Convertir a WebP
                var webpBytes = await _imageProcessing.ConvertToWebPAsync(file, quality);
                
                // Retornar la imagen convertida
                return File(webpBytes, "image/webp", $"preview_{Path.GetFileNameWithoutExtension(file.FileName)}.webp");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir imagen para preview");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        #region Métodos privados para validación de URLs

        private string DetectMediaTypeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "image";

            url = url.ToLowerInvariant();

            if (url.Contains("youtube.com") || url.Contains("youtu.be") || url.Contains("vimeo.com"))
                return "video";

            if (url.Contains("matterport.com") || url.Contains("360") || url.Contains("tour"))
                return "tour";

            if (url.Contains("drive.google.com"))
            {
                if (url.Contains("video") || url.Contains("mp4"))
                    return "video";
                return "image";
            }

            return "image";
        }

        private bool IsSupportedExternalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            url = url.ToLowerInvariant();

            // URLs soportadas
            var supportedDomains = new[]
            {
                "drive.google.com",
                "docs.google.com", 
                "youtube.com",
                "youtu.be",
                "vimeo.com",
                "player.vimeo.com",
                "matterport.com",
                "imgur.com",
                "dropbox.com"
            };

            return supportedDomains.Any(domain => url.Contains(domain));
        }

        private string ProcessUrlForPreview(string url)
        {
            // Convertir URLs de Google Drive para preview
            if (url.Contains("drive.google.com/file/d/"))
            {
                var fileId = System.Text.RegularExpressions.Regex.Match(url, @"/file/d/([a-zA-Z0-9-_]+)")?.Groups[1]?.Value;
                if (!string.IsNullOrEmpty(fileId))
                {
                    return $"https://drive.google.com/uc?export=view&id={fileId}";
                }
            }

            return url;
        }

        #endregion
    }
}