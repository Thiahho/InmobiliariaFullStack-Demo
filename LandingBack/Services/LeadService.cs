using AutoMapper;
using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class LeadService : ILeadService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<LeadService> _logger;

        public LeadService(AppDbContext context, IMapper mapper, ILogger<LeadService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LeadResponseDto> GetLeadByIdAsync(int id)
        {
            try
            {
                var lead = await _context.Leads
                    .Include(l => l.Propiedad)
                    .Include(l => l.AgenteAsignado)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lead == null)
                    throw new ArgumentException($"No existe lead con ID: {id}");

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lead {Id}", id);
                throw new InvalidOperationException($"Error al obtener lead: {ex.Message}", ex);
            }
        }

        public async Task<(IEnumerable<LeadResponseDto> Leads, int TotalCount)> GetLeadsAsync(LeadSearchDto searchDto)
        {
            try
            {
                var query = _context.Leads
                    .Include(l => l.Propiedad)
                    .Include(l => l.AgenteAsignado)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(searchDto.Estado))
                    query = query.Where(l => l.Estado == searchDto.Estado);

                if (searchDto.AgenteAsignadoId.HasValue)
                    query = query.Where(l => l.AgenteAsignadoId == searchDto.AgenteAsignadoId.Value);

                if (searchDto.PropiedadId.HasValue)
                    query = query.Where(l => l.PropiedadId == searchDto.PropiedadId.Value);

                if (!string.IsNullOrEmpty(searchDto.TipoConsulta))
                    query = query.Where(l => l.TipoConsulta == searchDto.TipoConsulta);

                if (searchDto.FechaDesde.HasValue)
                    query = query.Where(l => l.FechaCreacion >= searchDto.FechaDesde.Value);

                if (searchDto.FechaHasta.HasValue)
                    query = query.Where(l => l.FechaCreacion <= searchDto.FechaHasta.Value);

                // Ordenamiento
                query = searchDto.OrderBy?.ToLower() switch
                {
                    "nombre" => searchDto.OrderDesc ? query.OrderByDescending(l => l.Nombre) : query.OrderBy(l => l.Nombre),
                    "email" => searchDto.OrderDesc ? query.OrderByDescending(l => l.Email) : query.OrderBy(l => l.Email),
                    "estado" => searchDto.OrderDesc ? query.OrderByDescending(l => l.Estado) : query.OrderBy(l => l.Estado),
                    "fechacreacion" => searchDto.OrderDesc ? query.OrderByDescending(l => l.FechaCreacion) : query.OrderBy(l => l.FechaCreacion),
                    _ => searchDto.OrderDesc ? query.OrderByDescending(l => l.FechaCreacion) : query.OrderBy(l => l.FechaCreacion)
                };

                // Conteo total
                var totalCount = await query.CountAsync();

                // Paginación
                var leads = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var leadsDto = leads.Select(MapToResponseDto).ToList();
                return (leadsDto, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar leads");
                throw new InvalidOperationException($"Error al buscar leads: {ex.Message}", ex);
            }
        }

        public async Task<LeadResponseDto> CreateLeadAsync(LeadCreateDto leadCreateDto)
        {
            try
            {
                // Validar que la propiedad existe
                if (!await IsValidPropiedadAsync(leadCreateDto.PropiedadId))
                    throw new ArgumentException($"No existe propiedad con ID: {leadCreateDto.PropiedadId}");

                // Verificar duplicados
                if (await IsDuplicateLeadAsync(leadCreateDto.Email, leadCreateDto.PropiedadId, TimeSpan.FromHours(24)))
                {
                    _logger.LogWarning("Lead duplicado detectado: {Email} para propiedad {PropiedadId}", leadCreateDto.Email, leadCreateDto.PropiedadId);
                    // Retornar el lead existente en lugar de crear uno nuevo
                    var existingLead = await _context.Leads
                        .Include(l => l.Propiedad)
                        .Include(l => l.AgenteAsignado)
                        .FirstOrDefaultAsync(l => l.Email == leadCreateDto.Email && l.PropiedadId == leadCreateDto.PropiedadId);
                    return MapToResponseDto(existingLead!);
                }

                var lead = new Lead
                {
                    PropiedadId = leadCreateDto.PropiedadId,
                    Nombre = leadCreateDto.Nombre,
                    Email = leadCreateDto.Email,
                    Telefono = leadCreateDto.Telefono,
                    Mensaje = leadCreateDto.Mensaje,
                    TipoConsulta = leadCreateDto.TipoConsulta,
                    Canal = leadCreateDto.Canal,
                    Origen = leadCreateDto.Origen,
                    IpAddress = leadCreateDto.IpAddress,
                    UserAgent = leadCreateDto.UserAgent,
                    Estado = "Nuevo",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                // Auto-asignar si hay agentes disponibles
                await AutoAssignLeadAsync(lead.Id);

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lead");
                throw new InvalidOperationException($"Error al crear lead: {ex.Message}", ex);
            }
        }

        public async Task<LeadResponseDto> UpdateLeadAsync(int id, LeadUpdateDto leadUpdateDto)
        {
            try
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id);
                if (lead == null)
                    throw new ArgumentException($"No existe lead con ID: {id}");

                if (!string.IsNullOrEmpty(leadUpdateDto.Estado))
                    lead.Estado = leadUpdateDto.Estado;

                if (leadUpdateDto.AgenteAsignadoId.HasValue)
                    lead.AgenteAsignadoId = leadUpdateDto.AgenteAsignadoId.Value;

                if (!string.IsNullOrEmpty(leadUpdateDto.NotasInternas))
                    lead.NotasInternas = leadUpdateDto.NotasInternas;

                lead.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lead {Id}", id);
                throw new InvalidOperationException($"Error al actualizar lead: {ex.Message}", ex);
            }
        }

        public async Task DeleteLeadAsync(int id)
        {
            try
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id);
                if (lead == null)
                    throw new ArgumentException($"No existe lead con ID: {id}");

                _context.Leads.Remove(lead);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lead {Id}", id);
                throw new InvalidOperationException($"Error al eliminar lead: {ex.Message}", ex);
            }
        }

        public async Task<LeadResponseDto> AssignLeadAsync(LeadAssignDto assignDto, int usuarioId)
        {
            try
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == assignDto.LeadId);
                if (lead == null)
                    throw new ArgumentException($"No existe lead con ID: {assignDto.LeadId}");

                var agente = await _context.Agentes.FirstOrDefaultAsync(a => a.Id == assignDto.AgenteId);
                if (agente == null)
                    throw new ArgumentException($"No existe agente con ID: {assignDto.AgenteId}");

                lead.AgenteAsignadoId = assignDto.AgenteId;
                lead.Estado = "EnProceso";
                lead.FechaActualizacion = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(assignDto.Notas))
                    lead.NotasInternas = assignDto.Notas;

                await _context.SaveChangesAsync();

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar lead {LeadId} a agente {AgenteId}", assignDto.LeadId, assignDto.AgenteId);
                throw new InvalidOperationException($"Error al asignar lead: {ex.Message}", ex);
            }
        }

        public async Task<LeadResponseDto> UpdateLeadStatusAsync(LeadStatusUpdateDto statusDto, int usuarioId)
        {
            try
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == statusDto.LeadId);
                if (lead == null)
                    throw new ArgumentException($"No existe lead con ID: {statusDto.LeadId}");

                lead.Estado = statusDto.Estado;
                lead.FechaActualizacion = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(statusDto.NotasInternas))
                    lead.NotasInternas = statusDto.NotasInternas;

                await _context.SaveChangesAsync();

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de lead {LeadId}", statusDto.LeadId);
                throw new InvalidOperationException($"Error al actualizar estado: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeadResponseDto>> GetLeadsByAgenteAsync(int agenteId)
        {
            try
            {
                var leads = await _context.Leads
                    .Include(l => l.Propiedad)
                    .Include(l => l.AgenteAsignado)
                    .Where(l => l.AgenteAsignadoId == agenteId)
                    .OrderByDescending(l => l.FechaCreacion)
                    .AsNoTracking()
                    .ToListAsync();

                return leads.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads del agente {AgenteId}", agenteId);
                throw new InvalidOperationException($"Error al obtener leads del agente: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeadResponseDto>> GetUnassignedLeadsAsync()
        {
            try
            {
                var leads = await _context.Leads
                    .Include(l => l.Propiedad)
                    .Where(l => l.AgenteAsignadoId == null)
                    .OrderByDescending(l => l.FechaCreacion)
                    .AsNoTracking()
                    .ToListAsync();

                return leads.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads sin asignar");
                throw new InvalidOperationException($"Error al obtener leads sin asignar: {ex.Message}", ex);
            }
        }

        public async Task<LeadStatsDto> GetLeadStatsAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var query = _context.Leads.AsQueryable();

                if (fechaDesde.HasValue)
                    query = query.Where(l => l.FechaCreacion >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(l => l.FechaCreacion <= fechaHasta.Value);

                var leads = await query.AsNoTracking().ToListAsync();

                var stats = new LeadStatsDto
                {
                    TotalLeads = leads.Count,
                    LeadsNuevos = leads.Count(l => l.Estado == "Nuevo"),
                    LeadsEnProceso = leads.Count(l => l.Estado == "EnProceso"),
                    LeadsCerrados = leads.Count(l => l.Estado == "Cerrado"),
                    LeadsSinAsignar = leads.Count(l => l.AgenteAsignadoId == null)
                };

                if (stats.TotalLeads > 0)
                    stats.TasaConversion = (double)stats.LeadsCerrados / stats.TotalLeads * 100;

                stats.LeadsPorCanal = leads
                    .GroupBy(l => l.Canal)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.LeadsPorOrigen = leads
                    .GroupBy(l => l.Origen)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.LeadsPorFecha = leads
                    .GroupBy(l => l.FechaCreacion.Date)
                    .Select(g => new LeadsByDateDto { Fecha = g.Key, Cantidad = g.Count() })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de leads");
                throw new InvalidOperationException($"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeadResponseDto>> GetLeadsByPropiedadAsync(int propiedadId)
        {
            try
            {
                var leads = await _context.Leads
                    .Include(l => l.Propiedad)
                    .Include(l => l.AgenteAsignado)
                    .Where(l => l.PropiedadId == propiedadId)
                    .OrderByDescending(l => l.FechaCreacion)
                    .AsNoTracking()
                    .ToListAsync();

                return leads.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads de propiedad {PropiedadId}", propiedadId);
                throw new InvalidOperationException($"Error al obtener leads de propiedad: {ex.Message}", ex);
            }
        }

        public async Task<bool> BulkActionAsync(BulkLeadActionDto bulkActionDto, int usuarioId)
        {
            try
            {
                var leads = await _context.Leads
                    .Where(l => bulkActionDto.LeadIds.Contains(l.Id))
                    .ToListAsync();

                foreach (var lead in leads)
                {
                    switch (bulkActionDto.Accion.ToLower())
                    {
                        case "asignar":
                            if (bulkActionDto.AgenteId.HasValue)
                            {
                                lead.AgenteAsignadoId = bulkActionDto.AgenteId.Value;
                                lead.Estado = "EnProceso";
                            }
                            break;

                        case "cambiarestado":
                            if (!string.IsNullOrEmpty(bulkActionDto.Estado))
                                lead.Estado = bulkActionDto.Estado;
                            break;

                        case "eliminar":
                            _context.Leads.Remove(lead);
                            break;
                    }

                    if (!string.IsNullOrEmpty(bulkActionDto.Notas))
                        lead.NotasInternas = bulkActionDto.Notas;

                    lead.FechaActualizacion = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en acción masiva de leads");
                throw new InvalidOperationException($"Error en acción masiva: {ex.Message}", ex);
            }
        }

        public async Task<int?> GetNextAvailableAgenteAsync()
        {
            try
            {
                // Lógica simple: asignar al agente con menos leads activos
                var agenteConMenosLeads = await _context.Agentes
                    .Where(a => a.Activo)
                    .Select(a => new { a.Id, LeadsActivos = a.Leads.Count(l => l.Estado != "Cerrado") })
                    .OrderBy(x => x.LeadsActivos)
                    .FirstOrDefaultAsync();

                return agenteConMenosLeads?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agente disponible");
                return null;
            }
        }

        public async Task AutoAssignLeadAsync(int leadId)
        {
            try
            {
                var agenteId = await GetNextAvailableAgenteAsync();
                if (agenteId.HasValue)
                {
                    var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == leadId);
                    if (lead != null && lead.AgenteAsignadoId == null)
                    {
                        lead.AgenteAsignadoId = agenteId.Value;
                        lead.Estado = "EnProceso";
                        lead.FechaActualizacion = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en auto-asignación de lead {LeadId}", leadId);
                // No lanzar excepción, solo log
            }
        }

        public async Task<bool> IsDuplicateLeadAsync(string email, int propiedadId, TimeSpan timeWindow)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
                return await _context.Leads.AnyAsync(l => 
                    l.Email == email && 
                    l.PropiedadId == propiedadId && 
                    l.FechaCreacion >= cutoffTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar lead duplicado");
                return false;
            }
        }

        public async Task<bool> IsValidPropiedadAsync(int propiedadId)
        {
            try
            {
                return await _context.Propiedades.AnyAsync(p => p.Id == propiedadId && p.Estado == "Activo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar propiedad {PropiedadId}", propiedadId);
                return false;
            }
        }

        #region Métodos privados

        private LeadResponseDto MapToResponseDto(Lead lead)
        {
            return new LeadResponseDto
            {
                Id = lead.Id,
                Nombre = lead.Nombre,
                Email = lead.Email,
                Telefono = lead.Telefono,
                Mensaje = lead.Mensaje,
                PropiedadId = lead.PropiedadId,
                PropiedadCodigo = lead.Propiedad?.Codigo ?? "",
                PropiedadDireccion = lead.Propiedad?.Direccion ?? "",
                TipoConsulta = lead.TipoConsulta,
                Estado = lead.Estado,
                AgenteAsignadoId = lead.AgenteAsignadoId,
                AgenteAsignadoNombre = lead.AgenteAsignado?.Nombre,
                FechaCreacion = lead.FechaCreacion,
                FechaActualizacion = lead.FechaActualizacion,
                IpAddress = lead.IpAddress,
                UserAgent = lead.UserAgent
            };
        }

        #endregion
    }
}