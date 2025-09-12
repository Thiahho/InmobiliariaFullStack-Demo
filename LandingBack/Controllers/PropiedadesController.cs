using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropiedadesController : ControllerBase
    {
        private readonly IPropiedadesService _propiedadesService;
        private readonly IAdvancedSearchService _advancedSearchService;
        private readonly ILogger<PropiedadesController> _logger;

        public PropiedadesController(IPropiedadesService propiedadesService, IAdvancedSearchService advancedSearchService, ILogger<PropiedadesController> logger)
        {
            _propiedadesService= propiedadesService;
            _advancedSearchService = advancedSearchService;
            _logger = logger;
        }

        // GET: api/propiedades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropiedadResponseDto>>> GetPropiedades()
        {
            try
            {
                var propiedades = await _propiedadesService.GetAllPropiedadesAsync();
                return Ok(propiedades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PropiedadResponseDto>>> SearchPropiedades(
            [FromQuery] string? ubicacion = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] string? tipo = null)
        {
            try
            {
                var propiedades = await _propiedadesService.GetPropiedadesByFiltroAsync(ubicacion, precioMin, precioMax, tipo);
                return Ok(propiedades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar propiedades");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/paginadas
        [HttpGet("paginadas")]
        public async Task<ActionResult<object>> GetPropiedadesPaginadas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                var propiedades = await _propiedadesService.GetPropiedadesPaginadasAsync(pagina, tamanoPagina);
                var totalCount = await _propiedadesService.GetTotalPropiedadesCountAsync();
                
                return Ok(new
                {
                    Data = propiedades,
                    TotalCount = totalCount,
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina,
                    TotalPaginas = (int)Math.Ceiling((double)totalCount / tamanoPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades paginadas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/propiedades
        [HttpPost]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<ActionResult<PropiedadCreateDto>> CreatePropiedad(PropiedadCreateDto propiedadCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nuevaPropiedad = await _propiedadesService.CreatePropiedadAsync(propiedadCreateDto);
                
                return CreatedAtAction(nameof(GetPropiedad), new { id = nuevaPropiedad.Id }, nuevaPropiedad);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/propiedades/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agente,Cargador")]
        public async Task<IActionResult> UpdatePropiedad(int id, PropiedadUpdateDto propiedadUpdateDto)
        {
            try
            {
                if (id != propiedadUpdateDto.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? usuarioId = null;
                if (int.TryParse(usuarioIdStr, out var parsedId))
                    usuarioId = parsedId;

                await _propiedadesService.UpdatePropiedadAsync(propiedadUpdateDto, usuarioId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar propiedad {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/propiedades/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePropiedad(int id)
        {
            try
            {
                await _propiedadesService.DeletePropiedadAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar propiedad {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/propiedades/buscar-avanzada
        [HttpPost("buscar-avanzada")]
        public async Task<ActionResult<object>> BuscarAvanzada(PropiedadSearchDto searchDto)
        {
            try
            {
                _logger.LogInformation("🔍 BÚSQUEDA AVANZADA INICIADA");
                _logger.LogInformation("📥 Parámetros recibidos: {@SearchDto}", searchDto);
                _logger.LogInformation("🔑 SearchTerm: '{SearchTerm}'", searchDto.SearchTerm ?? "NULL");
                _logger.LogInformation("📊 Page: {Page}, PageSize: {PageSize}", searchDto.Page, searchDto.PageSize);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState inválido: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("✅ ModelState válido, llamando al servicio...");
                var (propiedades, totalCount) = await _propiedadesService.SearchPropiedadesAsync(searchDto);
                
                _logger.LogInformation("📈 Resultados: {Count} propiedades encontradas de {Total} total", propiedades?.Count() ?? 0, totalCount);
                
                var response = new
                {
                    Data = propiedades,
                    TotalCount = totalCount,
                    Pagina = searchDto.Page,
                    TamanoPagina = searchDto.PageSize,
                    TotalPaginas = (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                    Filtros = new
                    {
                        searchDto.SearchTerm,
                        searchDto.Operacion,
                        searchDto.Tipo,
                        searchDto.Barrio,
                        searchDto.Comuna,
                        searchDto.PrecioMin,
                        searchDto.PrecioMax,
                        searchDto.Ambientes,
                        searchDto.Dormitorios,
                        searchDto.Cochera,
                        searchDto.Estado,
                        searchDto.Destacado
                    }
                };

                _logger.LogInformation("✅ Respuesta preparada: {TotalPages} páginas totales", response.TotalPaginas);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR en búsqueda avanzada: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/existe
        [HttpGet("existe")]
        public async Task<ActionResult<bool>> ExistePropiedad(
            [FromQuery] int id,
            [FromQuery] string codigo,
            [FromQuery] string barrio,
            [FromQuery] string comuna)
        {
            try
            {
                var existe = await _propiedadesService.ExistePropiedadAsync(id, codigo, barrio, comuna);
                return Ok(existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de propiedad");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/propiedades/search-advanced
        [HttpPost("search-advanced")]
        public async Task<ActionResult<object>> SearchAdvanced(AdvancedSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (propiedades, totalCount, stats) = await _advancedSearchService.BusquedaAvanzadaAsync(searchDto);
                
                return Ok(new
                {
                    propiedades,
                    totalCount,
                    stats,
                    page = searchDto.Page,
                    pageSize = searchDto.PageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda avanzada");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/autocomplete
        [HttpGet("autocomplete")]
        public async Task<ActionResult<IEnumerable<AutocompleteResultDto>>> Autocomplete([FromQuery] AutocompleteRequestDto autocompleteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var results = await _advancedSearchService.AutocompleteAsync(autocompleteDto);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en autocomplete");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/search-stats
        [HttpPost("search-stats")]
        public async Task<ActionResult<SearchStatsDto>> GetSearchStats(AdvancedSearchDto searchDto)
        {
            try
            {
                var stats = await _advancedSearchService.GetSearchStatsAsync(searchDto);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de búsqueda");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/propiedades/save-search
        [HttpPost("save-search")]
        [Authorize]
        public async Task<ActionResult<SavedSearchDto>> SaveSearch([FromBody] object request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Parse request body
                var requestObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(request.ToString() ?? "{}");
                var nombre = requestObj?.GetProperty("nombre").GetString() ?? "";
                var searchParams = System.Text.Json.JsonSerializer.Deserialize<AdvancedSearchDto>(
                    requestObj?.GetProperty("searchParams").ToString() ?? "{}");

                if (string.IsNullOrEmpty(nombre) || searchParams == null)
                    return BadRequest("Nombre y parámetros de búsqueda son requeridos");

                var savedSearch = await _advancedSearchService.SaveSearchAsync(usuarioId, nombre, searchParams);
                return Ok(savedSearch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar búsqueda");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/saved-searches
        [HttpGet("saved-searches")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SavedSearchDto>>> GetSavedSearches()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var searches = await _advancedSearchService.GetSavedSearchesAsync(usuarioId);
                return Ok(searches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsquedas guardadas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/propiedades/saved-searches/{id}
        [HttpDelete("saved-searches/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSavedSearch(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _advancedSearchService.DeleteSavedSearchAsync(id, usuarioId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar búsqueda guardada");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/propiedades/execute-saved-search/{id}
        [HttpPost("execute-saved-search/{id}")]
        [Authorize]
        public async Task<ActionResult<object>> ExecuteSavedSearch(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var (propiedades, totalCount) = await _advancedSearchService.ExecuteSavedSearchAsync(id, usuarioId);
                
                return Ok(new
                {
                    propiedades,
                    totalCount
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar búsqueda guardada");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/propiedades/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PropiedadResponseDto>> GetPropiedad(int id)
        {
            try
            {
                var propiedad = await _propiedadesService.GetPropiedadByIdAsync(id);
                return Ok(propiedad);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedad {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}