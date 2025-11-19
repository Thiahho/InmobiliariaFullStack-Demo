using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using LandingBack.Data.Modelos;
using Microsoft.EntityFrameworkCore;
using Azure;
using System.Threading;

namespace LandingBack.Services
{
    public class PropiedadesService : IPropiedadesService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PropiedadesService> _logger;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IGeoService _geoService;

        public PropiedadesService(AppDbContext appDbContext, IMapper mapper, ILogger<PropiedadesService> logger, IAuditoriaService auditoriaService, IGeoService geoService)
        {
            _appDbContext=appDbContext;
            _logger = logger;
            _mapper=mapper;
            _auditoriaService = auditoriaService;
            _geoService = geoService;
        }

        public async Task<PropiedadCreateDto> CreatePropiedadAsync(PropiedadCreateDto propiedadCreateDto)
        {
            try
            {
                if (string.IsNullOrEmpty(propiedadCreateDto.Codigo))
                    throw new ArgumentException("El codigo es requerido");
                if (string.IsNullOrEmpty(propiedadCreateDto.Tipo))
                    throw new ArgumentException("El tipo es requerido");
                if (string.IsNullOrEmpty(propiedadCreateDto.Operacion))
                    throw new ArgumentException("La operacion es requerida");
                if (string.IsNullOrEmpty(propiedadCreateDto.Barrio))
                    throw new ArgumentException("El barrio es requerido");
                if (string.IsNullOrEmpty(propiedadCreateDto.Comuna))
                    throw new ArgumentException("La comuna es requerida");
                if (string.IsNullOrEmpty(propiedadCreateDto.Direccion))
                    throw new ArgumentException("La direccion es requerida");
                if (propiedadCreateDto.Precio <= 0)
                    throw new ArgumentException("El precio debe ser mayor a 0");
                // Ambientes es opcional para ciertos tipos de propiedades
                if (propiedadCreateDto.Ambientes.HasValue && propiedadCreateDto.Ambientes <= 0)
                    throw new ArgumentException("Los ambientes deben ser mayor a 0 si se especifican");

                var entidad = _mapper.Map<Propiedad>(propiedadCreateDto);
                entidad.FechaPublicacionUtc = DateTime.UtcNow;
                // entidad.Geo = _geoService.CreatePoint(propiedadCreateDto.Latitud, propiedadCreateDto.Longitud);
                entidad.GeoLatitud = propiedadCreateDto.Latitud;
                entidad.GeoLongitud = propiedadCreateDto.Longitud;

                _appDbContext.Propiedades.Add(entidad);
                await _appDbContext.SaveChangesAsync();
                var dto = _mapper.Map<PropiedadCreateDto>(entidad);
                // var coords = _geoService.GetCoordinates(entidad.Geo);
                dto.Latitud = entidad.GeoLatitud;
                dto.Longitud = entidad.GeoLongitud;
                return dto;


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad. Datos recibidos: {@PropiedadData}", propiedadCreateDto);

                var innerException = ex.InnerException;
                var fullMessage = ex.Message;

                while (innerException != null)
                {
                    fullMessage += $" -> Inner: {innerException.Message}";
                    innerException = innerException.InnerException;
                }

                throw new InvalidOperationException($"Error al crear la propiedad: {fullMessage}", ex);
            }
        }


        public async Task DeletePropiedadAsync(int id)
        {
            try
            {
                var entidad = await _appDbContext.Propiedades.FirstOrDefaultAsync(p => p.Id == id);
                if (entidad == null)
                    throw new ArgumentException($"No existe la propiedad con ID: {id}");

                _appDbContext.Propiedades.Remove(entidad);
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al eliminar la propiedad: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistePropiedadAsync(int id, string codigo, string barrio, string comuna)
        {
            return await _appDbContext.Propiedades.AnyAsync(p=>p.Codigo== codigo && p.Barrio==barrio && p.Comuna== comuna);
        }

        public async Task<IEnumerable<PropiedadResponseDto>> GetAllPropiedadesAsync()
        {
            var propiedad = await _appDbContext.Propiedades
                .Include(p => p.Medias)
                .Where(p=>p.Estado =="Activo")
                .AsNoTracking()
                .ToListAsync();

            var propiedadesDto = _mapper.Map<List<PropiedadResponseDto>>(propiedad);
            
            // Mapear coordenadas manualmente
            foreach (var dto in propiedadesDto)
            {
                var prop = propiedad.FirstOrDefault(p => p.Id == dto.Id);
                if (prop != null)
                {
                    // var coords = _geoService.GetCoordinates(prop.Geo);
                    var coords = (Latitud: prop.GeoLatitud, Longitud: prop.GeoLongitud);
                    dto.Latitud = coords.Latitud;
                    dto.Longitud = coords.Longitud;
                }
            }
            
            return propiedadesDto;
        }

        public async Task<PropiedadResponseDto> GetPropiedadByIdAsync(int id)
        {
            try
            {
                var propiedad = await _appDbContext.Propiedades
                    .Include(p => p.Medias)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (propiedad == null)
                    throw new ArgumentException($"No existe la propiedad con ID: {id}");

                var dto = _mapper.Map<PropiedadResponseDto>(propiedad);
                // var coords = _geoService.GetCoordinates(propiedad.Geo);
                var coords = (Latitud: propiedad.GeoLatitud, Longitud: propiedad.GeoLongitud);
                dto.Latitud = coords.Latitud;
                dto.Longitud = coords.Longitud;
                return dto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener la propiedad: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PropiedadResponseDto>> GetPropiedadesByFiltroAsync(string? ubicacion = null, decimal? precioMin = null, decimal? precioMax = null, string? tipo = null)
        {
            try
            {
                var query = _appDbContext.Propiedades
                    .Include(p => p.Medias)
                    .Where(p => p.Estado == "Activo")
                    .AsQueryable();

                if (!string.IsNullOrEmpty(ubicacion))
                {
                    query = query.Where(p => p.Barrio.Contains(ubicacion) || p.Comuna.Contains(ubicacion) || p.Direccion.Contains(ubicacion));
                }

                if (!string.IsNullOrEmpty(tipo))
                {
                    query = query.Where(p => p.Tipo == tipo);
                }

                if (precioMin.HasValue)
                {
                    query = query.Where(p => p.Precio >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    query = query.Where(p => p.Precio <= precioMax.Value);
                }

                var propiedades = await query.AsNoTracking().ToListAsync();
                return _mapper.Map<List<PropiedadResponseDto>>(propiedades);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al filtrar propiedades: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PropiedadResponseDto>> GetPropiedadesPaginadasAsync(int pagina, int tamanoPagina)
        {
            try
            {
                _logger.LogInformation("🔍 SERVICE: Iniciando GetPropiedadesPaginadasAsync - Página: {Pagina}, Tamaño: {TamanoPagina}", pagina, tamanoPagina);

                if (pagina < 1) pagina = 1;
                if (tamanoPagina < 1) tamanoPagina = 10;

                _logger.LogInformation("🔍 SERVICE: Consultando base de datos...");

                var propiedades = await _appDbContext.Propiedades
                    .Include(p => p.Medias)
                    .Where(p => p.Estado == "Activo")
                    .OrderBy(p => p.Id)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("✅ SERVICE: Propiedades obtenidas de DB: {Count}", propiedades.Count);
                _logger.LogInformation("🔄 SERVICE: Mapeando a DTOs...");

                var result = _mapper.Map<List<PropiedadResponseDto>>(propiedades);

                _logger.LogInformation("✅ SERVICE: Mapeo completado: {Count} items", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SERVICE ERROR: {Message}, Inner: {InnerMessage}", ex.Message, ex.InnerException?.Message);
                throw new InvalidOperationException($"Error al obtener propiedades paginadas: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalPropiedadesCountAsync()
        {
            try
            {
                return await _appDbContext.Propiedades
                    .Where(p => p.Estado == "Activo")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener el conteo de propiedades: {ex.Message}", ex);
            }
        }

        public async Task UpdatePropiedadAsync(PropiedadUpdateDto propiedadUpdateDto, int? usuarioId = null)
        {
            try
            {
                var entidadAnterior = await _appDbContext.Propiedades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == propiedadUpdateDto.Id);

                if (entidadAnterior == null)
                    throw new ArgumentException($"No existe la propiedad con ID: {propiedadUpdateDto.Id}");

                var entidadNueva = await _appDbContext.Propiedades
                    .FirstOrDefaultAsync(p => p.Id == propiedadUpdateDto.Id);

                if (entidadNueva == null)
                    throw new ArgumentException($"No existe la propiedad con ID: {propiedadUpdateDto.Id}");

                _mapper.Map(propiedadUpdateDto, entidadNueva);
                // entidadNueva.Geo = _geoService.CreatePoint(propiedadUpdateDto.Latitud, propiedadUpdateDto.Longitud);
                entidadNueva.GeoLatitud = propiedadUpdateDto.Latitud;
                entidadNueva.GeoLongitud = propiedadUpdateDto.Longitud;

                // Registrar cambios antes de guardar
                await _auditoriaService.RegistrarCambiosPropiedadAsync(entidadAnterior, entidadNueva, usuarioId);

                _appDbContext.Propiedades.Update(entidadNueva);
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al actualizar la propiedad: {ex.Message}", ex);
            }
        }

        public async Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount)> SearchPropiedadesAsync(PropiedadSearchDto searchDto)
        {
            try
            {
                _logger.LogInformation("🔧 SERVICE: Iniciando búsqueda en PropiedadesService");
                _logger.LogInformation("🔧 SERVICE: SearchTerm recibido: '{SearchTerm}'", searchDto.SearchTerm ?? "NULL");
                
                var query = _appDbContext.Propiedades
                    .Include(p => p.Medias)
                    .AsQueryable();

                _logger.LogInformation("🔧 SERVICE: Query inicial creada");

                // Filtros
                // Búsqueda general por múltiples campos
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var searchTerm = searchDto.SearchTerm.ToLower();
                    _logger.LogInformation("🔍 SERVICE: Aplicando filtro SearchTerm: '{SearchTerm}' (lowercase)", searchTerm);
                    
                    query = query.Where(p => 
                        p.Codigo.ToLower().Contains(searchTerm) ||
                        p.Direccion.ToLower().Contains(searchTerm) ||
                        p.Barrio.ToLower().Contains(searchTerm) ||
                        p.Comuna.ToLower().Contains(searchTerm) ||
                        (p.Titulo != null && p.Titulo.ToLower().Contains(searchTerm)) ||
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(searchTerm))
                    );
                    
                    _logger.LogInformation("✅ SERVICE: Filtro SearchTerm aplicado");
                }
                else
                {
                    _logger.LogInformation("⚠️ SERVICE: SearchTerm está vacío o null, no se aplica filtro");
                }

                if (!string.IsNullOrEmpty(searchDto.Operacion))
                    query = query.Where(p => p.Operacion == searchDto.Operacion);

                if (!string.IsNullOrEmpty(searchDto.Tipo))
                    query = query.Where(p => p.Tipo == searchDto.Tipo);

                if (!string.IsNullOrEmpty(searchDto.Barrio))
                    query = query.Where(p => p.Barrio.Contains(searchDto.Barrio));

                if (!string.IsNullOrEmpty(searchDto.Comuna))
                    query = query.Where(p => p.Comuna.Contains(searchDto.Comuna));

                if (searchDto.PrecioMin.HasValue)
                    query = query.Where(p => p.Precio >= searchDto.PrecioMin.Value);

                if (searchDto.PrecioMax.HasValue)
                    query = query.Where(p => p.Precio <= searchDto.PrecioMax.Value);

                if (searchDto.Ambientes.HasValue)
                    query = query.Where(p => p.Ambientes == searchDto.Ambientes.Value);

                if (searchDto.Dormitorios.HasValue)
                    query = query.Where(p => p.Dormitorios >= searchDto.Dormitorios.Value);

                if (searchDto.Cochera.HasValue)
                    query = query.Where(p => p.Cochera == searchDto.Cochera.Value);

                if (!string.IsNullOrEmpty(searchDto.Estado))
                    query = query.Where(p => p.Estado == searchDto.Estado);

                if (searchDto.Destacado.HasValue)
                    query = query.Where(p => p.Destacado == searchDto.Destacado.Value);

                // Ordenamiento
                query = searchDto.OrderBy?.ToLower() switch
                {
                    "precio" => searchDto.OrderDesc ? query.OrderByDescending(p => p.Precio) : query.OrderBy(p => p.Precio),
                    "fechapublicacionutc" => searchDto.OrderDesc ? query.OrderByDescending(p => p.FechaPublicacionUtc) : query.OrderBy(p => p.FechaPublicacionUtc),
                    "destacado" => searchDto.OrderDesc ? query.OrderByDescending(p => p.Destacado).ThenByDescending(p => p.FechaPublicacionUtc) : query.OrderBy(p => p.Destacado).ThenBy(p => p.FechaPublicacionUtc),
                    _ => searchDto.OrderDesc ? query.OrderByDescending(p => p.FechaPublicacionUtc) : query.OrderBy(p => p.FechaPublicacionUtc)
                };

                // Conteo total antes de paginación
                _logger.LogInformation("📊 SERVICE: Ejecutando query.CountAsync()...");
                var totalCount = await query.CountAsync();
                _logger.LogInformation("📊 SERVICE: Total encontrado: {TotalCount}", totalCount);

                // Paginación
                _logger.LogInformation("📄 SERVICE: Aplicando paginación - Page: {Page}, PageSize: {PageSize}", searchDto.Page, searchDto.PageSize);
                var propiedades = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("📄 SERVICE: Propiedades obtenidas: {Count}", propiedades.Count);
                
                var propiedadesDto = _mapper.Map<List<PropiedadResponseDto>>(propiedades);
                _logger.LogInformation("🏠 SERVICE: Propiedades mapeadas a DTO: {Count}", propiedadesDto.Count);
                
                // Log algunos códigos de ejemplo
                var ejemplosCodigos = propiedadesDto.Take(3).Select(p => p.Codigo).ToList();
                _logger.LogInformation("🏷️ SERVICE: Ejemplos de códigos encontrados: {Codigos}", string.Join(", ", ejemplosCodigos));
                
                return (propiedadesDto, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al buscar propiedades: {ex.Message}", ex);
            }
        }
    }
}
