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

        public PropiedadesService(AppDbContext appDbContext, IMapper mapper, ILogger<PropiedadesService> logger)
        {
            _appDbContext=appDbContext;
            _logger = logger;
            _mapper=mapper;
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
                if (propiedadCreateDto.Ambientes <= 0)
                    throw new ArgumentException("Los ambientes deben ser mayor a 0");

                var entidad = _mapper.Map<Propiedad>(propiedadCreateDto);
                entidad.FechaPublicacionUtc = DateTime.UtcNow;

                _appDbContext.Propiedades.Add(entidad);
                await _appDbContext.SaveChangesAsync();
                return _mapper.Map<PropiedadCreateDto>(entidad);


            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Error al crear la propiedad: {ex.Message}", ex);
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

                return _mapper.Map<PropiedadResponseDto>(propiedad);
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
                if (pagina < 1) pagina = 1;
                if (tamanoPagina < 1) tamanoPagina = 10;

                var propiedades = await _appDbContext.Propiedades
                    .Include(p => p.Medias)
                    .Where(p => p.Estado == "Activo")
                    .OrderBy(p => p.Id)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<PropiedadResponseDto>>(propiedades);
            }
            catch (Exception ex)
            {
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

        public async Task UpdatePropiedadAsync(PropiedadUpdateDto propiedadUpdateDto)
        {
            try
            {
                var entidad = await _appDbContext.Propiedades
                    .FirstOrDefaultAsync(p => p.Id == propiedadUpdateDto.Id);

                if (entidad == null)
                    throw new ArgumentException($"No existe la propiedad con ID: {propiedadUpdateDto.Id}");

                _mapper.Map(propiedadUpdateDto, entidad);
                _appDbContext.Propiedades.Update(entidad);
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al actualizar la propiedad: {ex.Message}", ex);
            }
        }
    }
}
