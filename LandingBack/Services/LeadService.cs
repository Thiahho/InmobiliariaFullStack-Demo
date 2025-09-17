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
        private readonly IVisitaService _visitaService;
        private readonly IEmailService _emailService;

        public LeadService(AppDbContext context, IMapper mapper, ILogger<LeadService> logger, IVisitaService visitaService, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _visitaService = visitaService;
            _emailService = emailService;
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

                // DESACTIVADO: Auto-asignar si hay agentes disponibles
                // Los leads ahora deben asignarse manualmente desde el panel de administración
                // await AutoAssignLeadAsync(lead.Id);

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                // Enviar notificación de confirmación al cliente
                try
                {
                    await _emailService.SendLeadConfirmationEmailAsync(lead.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error enviando email de confirmación al cliente para lead {LeadId}", lead.Id);
                    // No fallar la creación del lead si falla el email
                }

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

                // Si es un lead de tipo "Visita", crear automáticamente la visita
                if (lead.TipoConsulta == "Visita")
                {
                    try
                    {
                        // Parsear fecha preferida del mensaje si está presente
                        var fechaVisita = ParseFechaPreferidaFromMessage(lead.Mensaje) ?? DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(10), DateTimeKind.Utc);

                        // Asegurar que esté en horario comercial
                        fechaVisita = AdjustToBusinessHours(fechaVisita);

                        var visitaDto = new VisitaCreateDto
                        {
                            PropiedadId = lead.PropiedadId,
                            AgenteId = assignDto.AgenteId,
                            ClienteNombre = lead.Nombre,
                            ClienteTelefono = lead.Telefono,
                            ClienteEmail = lead.Email,
                            FechaHora = fechaVisita,
                            DuracionMinutos = 60,
                            Observaciones = $"Visita generada automáticamente al asignar agente. Lead #{lead.Id}. {lead.Mensaje}"
                        };

                        var visita = await _visitaService.CreateVisitaAsync(visitaDto, usuarioId);

                        // Actualizar notas del lead con la información de la visita
                        var notaVisita = $"Visita #{visita.Id} creada automáticamente para {fechaVisita:dd/MM/yyyy HH:mm}";
                        lead.NotasInternas = string.IsNullOrEmpty(lead.NotasInternas)
                            ? notaVisita
                            : lead.NotasInternas + "\n\n" + notaVisita;

                        await _context.SaveChangesAsync();

                        // Enviar notificación al agente
                        await _visitaService.SendVisitaNotificationAsync(visita.Id);

                        _logger.LogInformation("Visita {VisitaId} creada automáticamente al asignar agente {AgenteId} al lead {LeadId}",
                            visita.Id, assignDto.AgenteId, lead.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear visita automática al asignar agente para lead {LeadId}", lead.Id);
                        // No fallar la asignación si no se puede crear la visita
                    }
                }

                // Recargar con navegación
                await _context.Entry(lead)
                    .Reference(l => l.Propiedad)
                    .LoadAsync();
                await _context.Entry(lead)
                    .Reference(l => l.AgenteAsignado)
                    .LoadAsync();

                // Enviar notificación de asignación al agente
                try
                {
                    await _emailService.SendLeadAssignmentEmailAsync(lead.Id, assignDto.AgenteId, usuarioId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error enviando email de asignación al agente para lead {LeadId}", lead.Id);
                    // No fallar la asignación si falla el email
                }

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

        public async Task<(LeadResponseDto Lead, int? VisitaId)> CreateLeadWithVisitaAsync(LeadCreateDto leadCreateDto)
        {
            try
            {
                // Crear el lead primero
                var lead = await CreateLeadAsync(leadCreateDto);

                int? visitaId = null;

                // Si es una solicitud de visita, crear la visita automáticamente
                if (leadCreateDto.TipoConsulta == "Visita" && lead.AgenteAsignadoId.HasValue)
                {
                    try
                    {
                        // Parsear fecha preferida del mensaje si está presente
                        var fechaVisita = ParseFechaPreferidaFromMessage(leadCreateDto.Mensaje) ?? DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(10), DateTimeKind.Utc);

                        // Asegurar que esté en horario comercial
                        fechaVisita = AdjustToBusinessHours(fechaVisita);

                        var visitaDto = new VisitaCreateDto
                        {
                            PropiedadId = leadCreateDto.PropiedadId,
                            AgenteId = lead.AgenteAsignadoId.Value,
                            ClienteNombre = leadCreateDto.Nombre,
                            ClienteTelefono = leadCreateDto.Telefono,
                            ClienteEmail = leadCreateDto.Email,
                            FechaHora = fechaVisita,
                            DuracionMinutos = 60,
                            Observaciones = $"Visita generada automáticamente desde lead. {leadCreateDto.Mensaje}"
                        };

                        var visita = await _visitaService.CreateVisitaAsync(visitaDto, 0); // 0 = sistema
                        visitaId = visita.Id;

                        // Actualizar el lead con la referencia a la visita
                        var leadEntity = await _context.Leads.FirstOrDefaultAsync(l => l.Id == lead.Id);
                        if (leadEntity != null)
                        {
                            leadEntity.NotasInternas = $"Visita #{visitaId} creada automáticamente para {fechaVisita:dd/MM/yyyy HH:mm}";
                            await _context.SaveChangesAsync();
                        }

                        // Enviar notificación al agente
                        await _visitaService.SendVisitaNotificationAsync(visita.Id);

                        _logger.LogInformation("Visita {VisitaId} creada automáticamente para lead {LeadId}", visitaId, lead.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear visita automática para lead {LeadId}", lead.Id);
                        // No fallar el lead si la visita no se puede crear
                    }
                }

                return (lead, visitaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lead con visita");
                throw new InvalidOperationException($"Error al crear lead con visita: {ex.Message}", ex);
            }
        }

        private DateTime? ParseFechaPreferidaFromMessage(string? mensaje)
        {
            if (string.IsNullOrEmpty(mensaje))
                return null;

            try
            {
                // Buscar patrón "para el DD/MM/YYYY a las HH:mm"
                var regex = new System.Text.RegularExpressions.Regex(@"para el (\d{2}/\d{2}/\d{4}) a las (\d{2}:\d{2}) (AM|PM)");
                var match = regex.Match(mensaje);

                if (match.Success)
                {
                    var fechaStr = match.Groups[1].Value;
                    var horaStr = match.Groups[2].Value;
                    var periodoStr = match.Groups[3].Value;

                    if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var fecha))
                    {
                        if (TimeSpan.TryParse(horaStr, out var hora))
                        {
                            // Ajustar AM/PM
                            if (periodoStr == "PM" && hora.Hours < 12)
                                hora = hora.Add(TimeSpan.FromHours(12));
                            else if (periodoStr == "AM" && hora.Hours == 12)
                                hora = hora.Subtract(TimeSpan.FromHours(12));

                            var result = fecha.Add(hora);
                            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al parsear fecha preferida del mensaje: {Mensaje}", mensaje);
            }

            return null;
        }

        private DateTime AdjustToBusinessHours(DateTime fechaHora)
        {
            // Horario comercial: 8:00 - 18:00, Lunes a Sábado
            var fecha = fechaHora.Date;
            var hora = fechaHora.TimeOfDay;

            // Si es domingo, mover al lunes
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
                fecha = fecha.AddDays(1);

            // Si está fuera del horario comercial, ajustar a 10:00 AM
            if (hora < TimeSpan.FromHours(8) || hora > TimeSpan.FromHours(18))
                hora = TimeSpan.FromHours(10);

            // Asegurar que el DateTime esté en UTC
            var result = fecha.Add(hora);
            return result.Kind == DateTimeKind.Utc ? result : DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }

        #endregion
    }
}