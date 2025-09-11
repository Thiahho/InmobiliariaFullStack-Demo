using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LandingBack.Services
{
    public class AdvancedSearchService : IAdvancedSearchService
    {
        private readonly AppDbContext _context;

        public AdvancedSearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount, SearchStatsDto Stats)> BusquedaAvanzadaAsync(AdvancedSearchDto searchDto)
        {
            var query = BuildSearchQuery(searchDto);

            // Obtener estadísticas antes de paginar
            var stats = await GetSearchStatsFromQuery(query);

            // Aplicar ordenamiento
            query = ApplyOrdering(query, searchDto.OrderBy, searchDto.OrderDesc);

            // Obtener total para paginación
            var totalCount = await query.CountAsync();

            // Aplicar paginación
            var propiedades = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var propiedadesDto = propiedades.Select(MapToPropiedadResponseDto);

            return (propiedadesDto, totalCount, stats);
        }

        public async Task<IEnumerable<AutocompleteResultDto>> AutocompleteAsync(AutocompleteDto autocompleteDto)
        {
            var results = new List<AutocompleteResultDto>();
            var query = autocompleteDto.Query.ToLower();

            switch (autocompleteDto.Tipo?.ToLower())
            {
                case "direccion":
                    results.AddRange(await GetDireccionAutocomplete(query, autocompleteDto.Limit));
                    break;
                case "barrio":
                    results.AddRange(await GetBarrioAutocomplete(query, autocompleteDto.Limit));
                    break;
                case "codigo":
                    results.AddRange(await GetCodigoAutocomplete(query, autocompleteDto.Limit));
                    break;
                default:
                    results.AddRange(await GetGeneralAutocomplete(query, autocompleteDto.Limit));
                    break;
            }

            return results.Take(autocompleteDto.Limit);
        }

        public async Task<SearchStatsDto> GetSearchStatsAsync(AdvancedSearchDto searchDto)
        {
            var query = BuildSearchQuery(searchDto);
            return await GetSearchStatsFromQuery(query);
        }

        public async Task<SavedSearchDto> SaveSearchAsync(int usuarioId, string nombre, AdvancedSearchDto searchParams)
        {
            var savedSearch = new SavedSearch
            {
                UsuarioId = usuarioId,
                Nombre = nombre,
                ParametrosBusqueda = JsonSerializer.Serialize(searchParams),
                NotificacionesActivas = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavedSearches.Add(savedSearch);
            await _context.SaveChangesAsync();

            return new SavedSearchDto
            {
                Id = savedSearch.Id,
                UsuarioId = savedSearch.UsuarioId,
                Nombre = savedSearch.Nombre,
                ParametrosBusqueda = savedSearch.ParametrosBusqueda,
                NotificacionesActivas = savedSearch.NotificacionesActivas,
                FechaCreacion = savedSearch.FechaCreacion,
                UltimaEjecucion = savedSearch.UltimaEjecucion,
                ResultadosUltimaEjecucion = savedSearch.ResultadosUltimaEjecucion
            };
        }

        public async Task<IEnumerable<SavedSearchDto>> GetSavedSearchesAsync(int usuarioId)
        {
            var searches = await _context.SavedSearches
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return searches.Select(s => new SavedSearchDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                Nombre = s.Nombre,
                ParametrosBusqueda = s.ParametrosBusqueda,
                NotificacionesActivas = s.NotificacionesActivas,
                FechaCreacion = s.FechaCreacion,
                UltimaEjecucion = s.UltimaEjecucion,
                ResultadosUltimaEjecucion = s.ResultadosUltimaEjecucion
            });
        }

        public async Task DeleteSavedSearchAsync(int searchId, int usuarioId)
        {
            var search = await _context.SavedSearches
                .FirstOrDefaultAsync(s => s.Id == searchId && s.UsuarioId == usuarioId);

            if (search != null)
            {
                _context.SavedSearches.Remove(search);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<PropiedadResponseDto> Propiedades, int TotalCount)> ExecuteSavedSearchAsync(int searchId, int usuarioId)
        {
            var savedSearch = await _context.SavedSearches
                .FirstOrDefaultAsync(s => s.Id == searchId && s.UsuarioId == usuarioId);

            if (savedSearch == null)
                throw new KeyNotFoundException("Búsqueda guardada no encontrada");

            var searchParams = JsonSerializer.Deserialize<AdvancedSearchDto>(savedSearch.ParametrosBusqueda);
            if (searchParams == null)
                throw new InvalidOperationException("Parámetros de búsqueda inválidos");

            var (propiedades, totalCount, _) = await BusquedaAvanzadaAsync(searchParams);

            // Actualizar estadísticas de la búsqueda guardada
            savedSearch.UltimaEjecucion = DateTime.UtcNow;
            savedSearch.ResultadosUltimaEjecucion = totalCount;
            await _context.SaveChangesAsync();

            return (propiedades, totalCount);
        }

        private IQueryable<Propiedad> BuildSearchQuery(AdvancedSearchDto searchDto)
        {
            var query = _context.Propiedades
                .Include(p => p.PropiedadMedias)
                .AsQueryable();

            // Filtro de estado (incluir inactivas solo si se especifica)
            if (!searchDto.IncluirInactivas)
            {
                query = query.Where(p => p.Estado == "Activo");
            }

            // Búsqueda por texto
            if (!string.IsNullOrEmpty(searchDto.TextoBusqueda))
            {
                var texto = searchDto.TextoBusqueda.ToLower();
                query = query.Where(p => 
                    p.Titulo.ToLower().Contains(texto) ||
                    p.Descripcion.ToLower().Contains(texto) ||
                    p.Direccion.ToLower().Contains(texto) ||
                    p.Barrio.ToLower().Contains(texto) ||
                    p.Localidad.ToLower().Contains(texto));
            }

            // Filtros específicos
            if (!string.IsNullOrEmpty(searchDto.Codigo))
                query = query.Where(p => p.Codigo.Contains(searchDto.Codigo));

            if (!string.IsNullOrEmpty(searchDto.Direccion))
                query = query.Where(p => p.Direccion.ToLower().Contains(searchDto.Direccion.ToLower()));

            if (!string.IsNullOrEmpty(searchDto.TipoPropiedad))
                query = query.Where(p => p.TipoPropiedad == searchDto.TipoPropiedad);

            if (!string.IsNullOrEmpty(searchDto.TipoOperacion))
                query = query.Where(p => p.TipoOperacion == searchDto.TipoOperacion);

            // Filtros de precio
            if (searchDto.PrecioMin.HasValue)
                query = query.Where(p => p.Precio >= searchDto.PrecioMin.Value);

            if (searchDto.PrecioMax.HasValue)
                query = query.Where(p => p.Precio <= searchDto.PrecioMax.Value);

            // Filtros de características
            if (searchDto.AmbientesMin.HasValue)
                query = query.Where(p => p.Ambientes >= searchDto.AmbientesMin.Value);

            if (searchDto.AmbientesMax.HasValue)
                query = query.Where(p => p.Ambientes <= searchDto.AmbientesMax.Value);

            if (searchDto.DormitoriosMin.HasValue)
                query = query.Where(p => p.Dormitorios >= searchDto.DormitoriosMin.Value);

            if (searchDto.DormitoriosMax.HasValue)
                query = query.Where(p => p.Dormitorios <= searchDto.DormitoriosMax.Value);

            if (searchDto.BañosMin.HasValue)
                query = query.Where(p => p.Baños >= searchDto.BañosMin.Value);

            if (searchDto.BañosMax.HasValue)
                query = query.Where(p => p.Baños <= searchDto.BañosMax.Value);

            // Filtros de superficie
            if (searchDto.SuperficieCubiertaMin.HasValue)
                query = query.Where(p => p.SuperficieCubierta >= searchDto.SuperficieCubiertaMin.Value);

            if (searchDto.SuperficieCubiertaMax.HasValue)
                query = query.Where(p => p.SuperficieCubierta <= searchDto.SuperficieCubiertaMax.Value);

            if (searchDto.SuperficieTotalMin.HasValue)
                query = query.Where(p => p.SuperficieTotal >= searchDto.SuperficieTotalMin.Value);

            if (searchDto.SuperficieTotalMax.HasValue)
                query = query.Where(p => p.SuperficieTotal <= searchDto.SuperficieTotalMax.Value);

            // Filtros de antigüedad
            if (searchDto.AntiguedadMin.HasValue)
                query = query.Where(p => p.Antiguedad >= searchDto.AntiguedadMin.Value);

            if (searchDto.AntiguedadMax.HasValue)
                query = query.Where(p => p.Antiguedad <= searchDto.AntiguedadMax.Value);

            // Filtros geográficos
            query = ApplyGeographicFilters(query, searchDto.FiltroGeografico);

            // Filtros de amenities
            if (searchDto.Amenities?.Any() == true)
            {
                foreach (var amenity in searchDto.Amenities)
                {
                    query = query.Where(p => p.Amenities.Contains(amenity));
                }
            }

            // Filtros adicionales
            if (!string.IsNullOrEmpty(searchDto.Orientacion))
                query = query.Where(p => p.Orientacion == searchDto.Orientacion);

            if (searchDto.DisponibilidadInmediata.HasValue)
                query = query.Where(p => p.DisponibilidadInmediata == searchDto.DisponibilidadInmediata.Value);

            if (searchDto.AceptaMascotas.HasValue)
                query = query.Where(p => p.AceptaMascotas == searchDto.AceptaMascotas.Value);

            if (searchDto.AceptaCredito.HasValue)
                query = query.Where(p => p.AceptaCredito == searchDto.AceptaCredito.Value);

            // Filtros de fecha
            if (searchDto.FechaPublicacionDesde.HasValue)
                query = query.Where(p => p.FechaCreacion >= searchDto.FechaPublicacionDesde.Value);

            if (searchDto.FechaPublicacionHasta.HasValue)
                query = query.Where(p => p.FechaCreacion <= searchDto.FechaPublicacionHasta.Value);

            return query;
        }

        private IQueryable<Propiedad> ApplyGeographicFilters(IQueryable<Propiedad> query, GeoSearchDto? geoSearch)
        {
            if (geoSearch == null) return query;

            // Búsqueda por ubicación específica
            if (!string.IsNullOrEmpty(geoSearch.Barrio))
                query = query.Where(p => p.Barrio.ToLower() == geoSearch.Barrio.ToLower());

            if (!string.IsNullOrEmpty(geoSearch.Localidad))
                query = query.Where(p => p.Localidad.ToLower() == geoSearch.Localidad.ToLower());

            if (!string.IsNullOrEmpty(geoSearch.Provincia))
                query = query.Where(p => p.Provincia.ToLower() == geoSearch.Provincia.ToLower());

            // Búsqueda por radio
            if (geoSearch.Centro != null && geoSearch.RadioKm.HasValue)
            {
                query = ApplyRadiusFilter(query, geoSearch.Centro, geoSearch.RadioKm.Value);
            }

            // Búsqueda por polígono
            if (geoSearch.Poligono?.Any() == true)
            {
                query = ApplyPolygonFilter(query, geoSearch.Poligono);
            }

            return query;
        }

        private IQueryable<Propiedad> ApplyRadiusFilter(IQueryable<Propiedad> query, GeoPointDto centro, double radioKm)
        {
            // Implementación simplificada usando la fórmula de Haversine
            var lat1 = centro.Latitud;
            var lon1 = centro.Longitud;

            return query.Where(p => p.Latitud.HasValue && p.Longitud.HasValue &&
                (6371 * Math.Acos(
                    Math.Cos(lat1 * Math.PI / 180) * 
                    Math.Cos(p.Latitud.Value * Math.PI / 180) * 
                    Math.Cos((p.Longitud.Value - lon1) * Math.PI / 180) + 
                    Math.Sin(lat1 * Math.PI / 180) * 
                    Math.Sin(p.Latitud.Value * Math.PI / 180)
                )) <= radioKm);
        }

        private IQueryable<Propiedad> ApplyPolygonFilter(IQueryable<Propiedad> query, List<GeoPointDto> poligono)
        {
            // Implementación simplificada - en producción usar PostGIS o similar
            if (poligono.Count < 3) return query;

            var minLat = poligono.Min(p => p.Latitud);
            var maxLat = poligono.Max(p => p.Latitud);
            var minLon = poligono.Min(p => p.Longitud);
            var maxLon = poligono.Max(p => p.Longitud);

            return query.Where(p => p.Latitud.HasValue && p.Longitud.HasValue &&
                p.Latitud.Value >= minLat && p.Latitud.Value <= maxLat &&
                p.Longitud.Value >= minLon && p.Longitud.Value <= maxLon);
        }

        private IQueryable<Propiedad> ApplyOrdering(IQueryable<Propiedad> query, string? orderBy, bool orderDesc)
        {
            return orderBy?.ToLower() switch
            {
                "precio" => orderDesc ? query.OrderByDescending(p => p.Precio) : query.OrderBy(p => p.Precio),
                "superficie" => orderDesc ? query.OrderByDescending(p => p.SuperficieTotal) : query.OrderBy(p => p.SuperficieTotal),
                "ambientes" => orderDesc ? query.OrderByDescending(p => p.Ambientes) : query.OrderBy(p => p.Ambientes),
                "antiguedad" => orderDesc ? query.OrderByDescending(p => p.Antiguedad) : query.OrderBy(p => p.Antiguedad),
                "titulo" => orderDesc ? query.OrderByDescending(p => p.Titulo) : query.OrderBy(p => p.Titulo),
                "direccion" => orderDesc ? query.OrderByDescending(p => p.Direccion) : query.OrderBy(p => p.Direccion),
                _ => orderDesc ? query.OrderByDescending(p => p.FechaCreacion) : query.OrderBy(p => p.FechaCreacion)
            };
        }

        private async Task<SearchStatsDto> GetSearchStatsFromQuery(IQueryable<Propiedad> query)
        {
            var propiedades = await query.ToListAsync();

            if (!propiedades.Any())
            {
                return new SearchStatsDto
                {
                    TotalResultados = 0,
                    ResultadosPorTipo = new Dictionary<string, int>(),
                    ResultadosPorBarrio = new Dictionary<string, int>(),
                    ResultadosPorPrecio = new Dictionary<string, int>()
                };
            }

            var precios = propiedades.Select(p => p.Precio).ToList();

            return new SearchStatsDto
            {
                TotalResultados = propiedades.Count,
                PrecioPromedio = precios.Average(),
                PrecioMinimo = precios.Min(),
                PrecioMaximo = precios.Max(),
                ResultadosPorTipo = propiedades.GroupBy(p => p.TipoPropiedad)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ResultadosPorBarrio = propiedades.GroupBy(p => p.Barrio)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ResultadosPorPrecio = GetPriceRangeStats(precios)
            };
        }

        private Dictionary<string, int> GetPriceRangeStats(List<decimal> precios)
        {
            var ranges = new Dictionary<string, int>
            {
                ["0-50K"] = precios.Count(p => p < 50000),
                ["50K-100K"] = precios.Count(p => p >= 50000 && p < 100000),
                ["100K-200K"] = precios.Count(p => p >= 100000 && p < 200000),
                ["200K-500K"] = precios.Count(p => p >= 200000 && p < 500000),
                ["500K+"] = precios.Count(p => p >= 500000)
            };

            return ranges.Where(r => r.Value > 0).ToDictionary(r => r.Key, r => r.Value);
        }

        private async Task<List<AutocompleteResultDto>> GetDireccionAutocomplete(string query, int limit)
        {
            var propiedades = await _context.Propiedades
                .Where(p => p.Estado == "Activo" && p.Direccion.ToLower().Contains(query))
                .GroupBy(p => p.Direccion)
                .Select(g => new { Direccion = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync();

            return propiedades.Select(p => new AutocompleteResultDto
            {
                Valor = p.Direccion,
                Tipo = "direccion",
                Descripcion = $"{p.Count} propiedades",
                Coincidencias = p.Count
            }).ToList();
        }

        private async Task<List<AutocompleteResultDto>> GetBarrioAutocomplete(string query, int limit)
        {
            var barrios = await _context.Propiedades
                .Where(p => p.Estado == "Activo" && p.Barrio.ToLower().Contains(query))
                .GroupBy(p => new { p.Barrio, p.Localidad })
                .Select(g => new { g.Key.Barrio, g.Key.Localidad, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync();

            return barrios.Select(b => new AutocompleteResultDto
            {
                Valor = b.Barrio,
                Tipo = "barrio",
                Descripcion = $"{b.Localidad} - {b.Count} propiedades",
                Coincidencias = b.Count
            }).ToList();
        }

        private async Task<List<AutocompleteResultDto>> GetCodigoAutocomplete(string query, int limit)
        {
            var codigos = await _context.Propiedades
                .Where(p => p.Estado == "Activo" && p.Codigo.ToLower().Contains(query))
                .Select(p => new { p.Codigo, p.Titulo })
                .Take(limit)
                .ToListAsync();

            return codigos.Select(c => new AutocompleteResultDto
            {
                Valor = c.Codigo,
                Tipo = "codigo",
                Descripcion = c.Titulo,
                Coincidencias = 1
            }).ToList();
        }

        private async Task<List<AutocompleteResultDto>> GetGeneralAutocomplete(string query, int limit)
        {
            var results = new List<AutocompleteResultDto>();

            // Buscar en códigos
            results.AddRange(await GetCodigoAutocomplete(query, limit / 3));

            // Buscar en direcciones
            results.AddRange(await GetDireccionAutocomplete(query, limit / 3));

            // Buscar en barrios
            results.AddRange(await GetBarrioAutocomplete(query, limit / 3));

            return results.Take(limit).ToList();
        }

        private PropiedadResponseDto MapToPropiedadResponseDto(Propiedad propiedad)
        {
            return new PropiedadResponseDto
            {
                Id = propiedad.Id,
                Codigo = propiedad.Codigo,
                Titulo = propiedad.Titulo,
                Descripcion = propiedad.Descripcion,
                TipoPropiedad = propiedad.TipoPropiedad,
                TipoOperacion = propiedad.TipoOperacion,
                Precio = propiedad.Precio,
                Moneda = propiedad.Moneda,
                Direccion = propiedad.Direccion,
                Barrio = propiedad.Barrio,
                Localidad = propiedad.Localidad,
                Provincia = propiedad.Provincia,
                CodigoPostal = propiedad.CodigoPostal,
                Latitud = propiedad.Latitud,
                Longitud = propiedad.Longitud,
                Ambientes = propiedad.Ambientes,
                Dormitorios = propiedad.Dormitorios,
                Baños = propiedad.Baños,
                SuperficieCubierta = propiedad.SuperficieCubierta,
                SuperficieTotal = propiedad.SuperficieTotal,
                Antiguedad = propiedad.Antiguedad,
                Estado = propiedad.Estado,
                Amenities = propiedad.Amenities,
                Orientacion = propiedad.Orientacion,
                DisponibilidadInmediata = propiedad.DisponibilidadInmediata,
                AceptaMascotas = propiedad.AceptaMascotas,
                AceptaCredito = propiedad.AceptaCredito,
                FechaCreacion = propiedad.FechaCreacion,
                FechaActualizacion = propiedad.FechaActualizacion,
                Medias = propiedad.PropiedadMedias?.Select(m => new PropiedadMediaDto
                {
                    Id = m.Id,
                    Titulo = m.Titulo,
                    Url = m.Url,
                    TipoArchivo = m.TipoArchivo,
                    EsPrincipal = m.EsPrincipal,
                    Orden = m.Orden
                }).ToList() ?? new List<PropiedadMediaDto>()
            };
        }
    }
}