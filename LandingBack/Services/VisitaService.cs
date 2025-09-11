using AutoMapper;
using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class VisitaService : IVisitaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IVisitaAuditoriaService _auditoriaService;

        public VisitaService(AppDbContext context, IMapper mapper, IVisitaAuditoriaService auditoriaService)
        {
            _context = context;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
        }

        public async Task<VisitaResponseDto> GetVisitaByIdAsync(int id)
        {
            var visita = await _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {id} no encontrada");

            return MapToResponseDto(visita);
        }

        public async Task<(IEnumerable<VisitaResponseDto> Visitas, int TotalCount)> GetVisitasAsync(VisitaSearchDto searchDto)
        {
            var query = _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .AsQueryable();

            if (searchDto.AgenteId.HasValue)
                query = query.Where(v => v.AgenteId == searchDto.AgenteId.Value);

            if (searchDto.PropiedadId.HasValue)
                query = query.Where(v => v.PropiedadId == searchDto.PropiedadId.Value);

            if (!string.IsNullOrEmpty(searchDto.Estado))
                query = query.Where(v => v.Estado == searchDto.Estado);

            if (searchDto.FechaDesde.HasValue)
                query = query.Where(v => v.FechaHora >= searchDto.FechaDesde.Value);

            if (searchDto.FechaHasta.HasValue)
                query = query.Where(v => v.FechaHora <= searchDto.FechaHasta.Value);

            var totalCount = await query.CountAsync();

            var orderBy = searchDto.OrderBy?.ToLower() ?? "fechahora";
            query = orderBy switch
            {
                "fechahora" => searchDto.OrderDesc ? query.OrderByDescending(v => v.FechaHora) : query.OrderBy(v => v.FechaHora),
                "estado" => searchDto.OrderDesc ? query.OrderByDescending(v => v.Estado) : query.OrderBy(v => v.Estado),
                "agente" => searchDto.OrderDesc ? query.OrderByDescending(v => v.Agente.Nombre) : query.OrderBy(v => v.Agente.Nombre),
                _ => query.OrderBy(v => v.FechaHora)
            };

            var visitas = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return (visitas.Select(MapToResponseDto), totalCount);
        }

        public async Task<VisitaResponseDto> CreateVisitaAsync(VisitaCreateDto visitaCreateDto, int creadoPorUsuarioId)
        {
            if (!await IsValidTimeSlotAsync(visitaCreateDto.AgenteId, visitaCreateDto.FechaHora, visitaCreateDto.DuracionMinutos))
                throw new InvalidOperationException("El horario seleccionado no está disponible");

            if (await CheckConflictAsync(new ConflictCheckDto
            {
                AgenteId = visitaCreateDto.AgenteId,
                FechaHora = visitaCreateDto.FechaHora,
                DuracionMinutos = visitaCreateDto.DuracionMinutos
            }))
                throw new InvalidOperationException("Existe un conflicto de horario con otra visita");

            var visita = new Visita
            {
                PropiedadId = visitaCreateDto.PropiedadId,
                AgenteId = visitaCreateDto.AgenteId,
                ClienteNombre = visitaCreateDto.ClienteNombre,
                ClienteTelefono = visitaCreateDto.ClienteTelefono,
                ClienteEmail = visitaCreateDto.ClienteEmail,
                FechaHora = visitaCreateDto.FechaHora,
                DuracionMinutos = visitaCreateDto.DuracionMinutos,
                Observaciones = visitaCreateDto.Observaciones,
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow,
                CreadoPorUsuarioId = creadoPorUsuarioId
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(creadoPorUsuarioId);
            await _auditoriaService.LogActionAsync(
                visita.Id, 
                "Crear", 
                creadoPorUsuarioId, 
                usuario?.Nombre ?? "Sistema",
                null,
                visitaCreateDto,
                "Visita creada"
            );

            return await GetVisitaByIdAsync(visita.Id);
        }

        public async Task<VisitaResponseDto> UpdateVisitaAsync(int id, VisitaUpdateDto visitaUpdateDto, int usuarioId)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {id} no encontrada");

            var valoresAnteriores = new
            {
                visita.PropiedadId,
                visita.AgenteId,
                visita.ClienteNombre,
                visita.ClienteTelefono,
                visita.ClienteEmail,
                visita.FechaHora,
                visita.DuracionMinutos,
                visita.Observaciones,
                visita.Estado,
                visita.NotasVisita
            };

            if (visitaUpdateDto.FechaHora != visita.FechaHora || 
                visitaUpdateDto.DuracionMinutos != visita.DuracionMinutos ||
                visitaUpdateDto.AgenteId != visita.AgenteId)
            {
                if (await CheckConflictAsync(new ConflictCheckDto
                {
                    AgenteId = visitaUpdateDto.AgenteId,
                    FechaHora = visitaUpdateDto.FechaHora,
                    DuracionMinutos = visitaUpdateDto.DuracionMinutos,
                    VisitaIdExcluir = id
                }))
                    throw new InvalidOperationException("Existe un conflicto de horario con otra visita");
            }

            visita.PropiedadId = visitaUpdateDto.PropiedadId;
            visita.AgenteId = visitaUpdateDto.AgenteId;
            visita.ClienteNombre = visitaUpdateDto.ClienteNombre;
            visita.ClienteTelefono = visitaUpdateDto.ClienteTelefono;
            visita.ClienteEmail = visitaUpdateDto.ClienteEmail;
            visita.FechaHora = visitaUpdateDto.FechaHora;
            visita.DuracionMinutos = visitaUpdateDto.DuracionMinutos;
            visita.Observaciones = visitaUpdateDto.Observaciones;
            
            if (!string.IsNullOrEmpty(visitaUpdateDto.Estado))
                visita.Estado = visitaUpdateDto.Estado;
            
            if (!string.IsNullOrEmpty(visitaUpdateDto.NotasVisita))
                visita.NotasVisita = visitaUpdateDto.NotasVisita;

            visita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(usuarioId);
            await _auditoriaService.LogActionAsync(
                id, 
                "Modificar", 
                usuarioId, 
                usuario?.Nombre ?? "Sistema",
                valoresAnteriores,
                visitaUpdateDto,
                "Visita actualizada"
            );

            return await GetVisitaByIdAsync(id);
        }

        public async Task DeleteVisitaAsync(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {id} no encontrada");

            _context.Visitas.Remove(visita);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<VisitaResponseDto>> GetVisitasByAgenteAsync(int agenteId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .Where(v => v.AgenteId == agenteId);

            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaHora <= fechaHasta.Value);

            var visitas = await query.OrderBy(v => v.FechaHora).ToListAsync();
            return visitas.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<VisitaResponseDto>> GetVisitasByPropiedadAsync(int propiedadId)
        {
            var visitas = await _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .Where(v => v.PropiedadId == propiedadId)
                .OrderBy(v => v.FechaHora)
                .ToListAsync();

            return visitas.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<VisitaCalendarDto>> GetVisitasCalendarAsync(int? agenteId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .AsQueryable();

            if (agenteId.HasValue)
                query = query.Where(v => v.AgenteId == agenteId.Value);

            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaHora <= fechaHasta.Value);

            var visitas = await query.ToListAsync();

            return visitas.Select(v => new VisitaCalendarDto
            {
                Id = v.Id,
                Title = $"{v.ClienteNombre} - {v.Propiedad.Codigo}",
                Start = v.FechaHora,
                End = v.FechaHora.AddMinutes(v.DuracionMinutos),
                Color = v.Estado switch
                {
                    "Pendiente" => "#ffc107",
                    "Confirmada" => "#007bff",
                    "Realizada" => "#28a745",
                    "Cancelada" => "#dc3545",
                    _ => "#6c757d"
                },
                Estado = v.Estado,
                Description = $"Cliente: {v.ClienteNombre}\nPropiedad: {v.Propiedad.Direccion}\nAgente: {v.Agente.Nombre}"
            });
        }

        public async Task<DisponibilidadDto> GetDisponibilidadAgenteAsync(int agenteId, DateTime fecha)
        {
            var agente = await _context.Agentes.FindAsync(agenteId);
            if (agente == null)
                throw new KeyNotFoundException($"Agente con ID {agenteId} no encontrado");

            var visitasDelDia = await _context.Visitas
                .Where(v => v.AgenteId == agenteId && 
                           v.FechaHora.Date == fecha.Date &&
                           v.Estado != "Cancelada")
                .ToListAsync();

            var slots = new List<TimeSlot>();
            var horaInicio = new TimeSpan(9, 0, 0);
            var horaFin = new TimeSpan(18, 0, 0);
            var duracionSlot = TimeSpan.FromMinutes(30);

            for (var hora = horaInicio; hora < horaFin; hora = hora.Add(duracionSlot))
            {
                var finSlot = hora.Add(duracionSlot);
                var fechaHoraSlot = fecha.Date.Add(hora);
                
                var ocupado = visitasDelDia.Any(v => 
                    v.FechaHora.TimeOfDay < finSlot && 
                    v.FechaHora.AddMinutes(v.DuracionMinutos).TimeOfDay > hora);

                slots.Add(new TimeSlot
                {
                    Inicio = hora,
                    Fin = finSlot,
                    Disponible = !ocupado && await IsBusinessHourAsync(fechaHoraSlot),
                    Motivo = ocupado ? "Ocupado" : null
                });
            }

            return new DisponibilidadDto
            {
                AgenteId = agenteId,
                Fecha = fecha,
                HorariosDisponibles = slots
            };
        }

        public async Task<bool> CheckConflictAsync(ConflictCheckDto conflictDto)
        {
            var query = _context.Visitas
                .Where(v => v.AgenteId == conflictDto.AgenteId &&
                           v.Estado != "Cancelada");

            if (conflictDto.VisitaIdExcluir.HasValue)
                query = query.Where(v => v.Id != conflictDto.VisitaIdExcluir.Value);

            var visitasExistentes = await query.ToListAsync();

            var inicioNuevaVisita = conflictDto.FechaHora;
            var finNuevaVisita = conflictDto.FechaHora.AddMinutes(conflictDto.DuracionMinutos);

            return visitasExistentes.Any(v =>
            {
                var inicioExistente = v.FechaHora;
                var finExistente = v.FechaHora.AddMinutes(v.DuracionMinutos);
                
                return inicioNuevaVisita < finExistente && finNuevaVisita > inicioExistente;
            });
        }

        public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int agenteId, DateTime fecha, int duracionMinutos = 60)
        {
            var disponibilidad = await GetDisponibilidadAgenteAsync(agenteId, fecha);
            var slotsDisponibles = new List<DateTime>();

            foreach (var slot in disponibilidad.HorariosDisponibles.Where(s => s.Disponible))
            {
                var fechaHoraSlot = fecha.Date.Add(slot.Inicio);
                
                if (!await CheckConflictAsync(new ConflictCheckDto
                {
                    AgenteId = agenteId,
                    FechaHora = fechaHoraSlot,
                    DuracionMinutos = duracionMinutos
                }))
                {
                    slotsDisponibles.Add(fechaHoraSlot);
                }
            }

            return slotsDisponibles;
        }

        public async Task<VisitaResponseDto> ConfirmarVisitaAsync(int visitaId, int usuarioId)
        {
            var visita = await _context.Visitas.FindAsync(visitaId);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {visitaId} no encontrada");

            if (visita.Estado != "Pendiente")
                throw new InvalidOperationException("Solo se pueden confirmar visitas pendientes");

            var estadoAnterior = visita.Estado;
            visita.Estado = "Confirmada";
            visita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(usuarioId);
            await _auditoriaService.LogActionAsync(
                visitaId, 
                "Confirmar", 
                usuarioId, 
                usuario?.Nombre ?? "Sistema",
                new { Estado = estadoAnterior },
                new { Estado = "Confirmada" },
                "Visita confirmada"
            );

            return await GetVisitaByIdAsync(visitaId);
        }

        public async Task<VisitaResponseDto> CancelarVisitaAsync(int visitaId, string motivo, int usuarioId)
        {
            var visita = await _context.Visitas.FindAsync(visitaId);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {visitaId} no encontrada");

            if (visita.Estado == "Realizada")
                throw new InvalidOperationException("No se puede cancelar una visita ya realizada");

            var estadoAnterior = visita.Estado;
            visita.Estado = "Cancelada";
            visita.NotasVisita = $"Cancelada: {motivo}";
            visita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(usuarioId);
            await _auditoriaService.LogActionAsync(
                visitaId, 
                "Cancelar", 
                usuarioId, 
                usuario?.Nombre ?? "Sistema",
                new { Estado = estadoAnterior },
                new { Estado = "Cancelada", Motivo = motivo },
                $"Visita cancelada: {motivo}"
            );

            return await GetVisitaByIdAsync(visitaId);
        }

        public async Task<VisitaResponseDto> ReagendarVisitaAsync(int visitaId, DateTime nuevaFecha, int usuarioId)
        {
            var visita = await _context.Visitas.FindAsync(visitaId);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {visitaId} no encontrada");

            if (await CheckConflictAsync(new ConflictCheckDto
            {
                AgenteId = visita.AgenteId,
                FechaHora = nuevaFecha,
                DuracionMinutos = visita.DuracionMinutos,
                VisitaIdExcluir = visitaId
            }))
                throw new InvalidOperationException("Existe un conflicto de horario en la nueva fecha");

            var fechaAnterior = visita.FechaHora;
            visita.FechaHora = nuevaFecha;
            visita.NotasVisita = $"Reagendada desde {fechaAnterior:dd/MM/yyyy HH:mm}";
            visita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(usuarioId);
            await _auditoriaService.LogActionAsync(
                visitaId, 
                "Reagendar", 
                usuarioId, 
                usuario?.Nombre ?? "Sistema",
                new { FechaHora = fechaAnterior },
                new { FechaHora = nuevaFecha },
                $"Visita reagendada desde {fechaAnterior:dd/MM/yyyy HH:mm} a {nuevaFecha:dd/MM/yyyy HH:mm}"
            );

            return await GetVisitaByIdAsync(visitaId);
        }

        public async Task<VisitaResponseDto> MarcarComoRealizadaAsync(int visitaId, string? notas, int usuarioId)
        {
            var visita = await _context.Visitas.FindAsync(visitaId);
            if (visita == null)
                throw new KeyNotFoundException($"Visita con ID {visitaId} no encontrada");

            if (visita.Estado != "Confirmada")
                throw new InvalidOperationException("Solo se pueden marcar como realizadas las visitas confirmadas");

            var estadoAnterior = visita.Estado;
            visita.Estado = "Realizada";
            if (!string.IsNullOrEmpty(notas))
                visita.NotasVisita = notas;
            visita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuario = await _context.Agentes.FindAsync(usuarioId);
            await _auditoriaService.LogActionAsync(
                visitaId, 
                "Realizada", 
                usuarioId, 
                usuario?.Nombre ?? "Sistema",
                new { Estado = estadoAnterior },
                new { Estado = "Realizada", Notas = notas },
                "Visita marcada como realizada"
            );

            return await GetVisitaByIdAsync(visitaId);
        }

        public async Task<bool> BulkActionAsync(BulkVisitaActionDto bulkActionDto, int usuarioId)
        {
            var visitas = await _context.Visitas
                .Where(v => bulkActionDto.VisitaIds.Contains(v.Id))
                .ToListAsync();

            if (!visitas.Any())
                return false;

            switch (bulkActionDto.Accion.ToLower())
            {
                case "confirmar":
                    foreach (var visita in visitas.Where(v => v.Estado == "Pendiente"))
                    {
                        visita.Estado = "Confirmada";
                        visita.FechaActualizacion = DateTime.UtcNow;
                    }
                    break;

                case "cancelar":
                    foreach (var visita in visitas.Where(v => v.Estado != "Realizada"))
                    {
                        visita.Estado = "Cancelada";
                        visita.NotasVisita = bulkActionDto.Motivo ?? "Cancelación masiva";
                        visita.FechaActualizacion = DateTime.UtcNow;
                    }
                    break;

                case "reagendar":
                    if (!bulkActionDto.NuevaFecha.HasValue)
                        throw new ArgumentException("Nueva fecha es requerida para reagendar");

                    foreach (var visita in visitas)
                    {
                        if (!await CheckConflictAsync(new ConflictCheckDto
                        {
                            AgenteId = visita.AgenteId,
                            FechaHora = bulkActionDto.NuevaFecha.Value,
                            DuracionMinutos = visita.DuracionMinutos,
                            VisitaIdExcluir = visita.Id
                        }))
                        {
                            visita.FechaHora = bulkActionDto.NuevaFecha.Value;
                            visita.NotasVisita = "Reagendada masivamente";
                            visita.FechaActualizacion = DateTime.UtcNow;
                        }
                    }
                    break;

                default:
                    throw new ArgumentException($"Acción '{bulkActionDto.Accion}' no reconocida");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VisitaStatsDto> GetVisitaStatsAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? agenteId = null)
        {
            var query = _context.Visitas.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaHora <= fechaHasta.Value);

            if (agenteId.HasValue)
                query = query.Where(v => v.AgenteId == agenteId.Value);

            var visitas = await query.Include(v => v.Agente).ToListAsync();

            var visitasPorAgente = visitas
                .GroupBy(v => v.Agente.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());

            var visitasPorFecha = visitas
                .GroupBy(v => v.FechaHora.Date)
                .Select(g => new VisitasByDateDto
                {
                    Fecha = g.Key,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            var totalVisitas = visitas.Count;
            var visitasRealizadas = visitas.Count(v => v.Estado == "Realizada");

            return new VisitaStatsDto
            {
                TotalVisitas = totalVisitas,
                VisitasPendientes = visitas.Count(v => v.Estado == "Pendiente"),
                VisitasConfirmadas = visitas.Count(v => v.Estado == "Confirmada"),
                VisitasRealizadas = visitasRealizadas,
                VisitasCanceladas = visitas.Count(v => v.Estado == "Cancelada"),
                TasaRealizacion = totalVisitas > 0 ? (double)visitasRealizadas / totalVisitas * 100 : 0,
                VisitasPorAgente = visitasPorAgente,
                VisitasPorFecha = visitasPorFecha
            };
        }

        public async Task SendVisitaNotificationAsync(int visitaId, string tipoNotificacion = "Email")
        {
            var visita = await GetVisitaByIdAsync(visitaId);
            
            // Aquí se implementaría la lógica de envío de notificaciones
            // Por ahora solo registramos que se debe enviar
            await Task.CompletedTask;
        }

        public async Task<string> GenerateICSFileAsync(int visitaId)
        {
            var visita = await GetVisitaByIdAsync(visitaId);
            
            var icsContent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Inmobiliaria//Visitas//ES
BEGIN:VEVENT
UID:{Guid.NewGuid()}@inmobiliaria.com
DTSTART:{visita.FechaHora:yyyyMMddTHHmmssZ}
DTEND:{visita.FechaHora.AddMinutes(visita.DuracionMinutos):yyyyMMddTHHmmssZ}
SUMMARY:Visita - {visita.PropiedadCodigo}
DESCRIPTION:Cliente: {visita.ClienteNombre}\nPropiedad: {visita.PropiedadDireccion}\nAgente: {visita.AgenteNombre}
LOCATION:{visita.PropiedadDireccion}
END:VEVENT
END:VCALENDAR";

            return icsContent;
        }

        public async Task<bool> IsValidTimeSlotAsync(int agenteId, DateTime fechaHora, int duracionMinutos)
        {
            if (!await IsBusinessHourAsync(fechaHora))
                return false;

            var finVisita = fechaHora.AddMinutes(duracionMinutos);
            if (!await IsBusinessHourAsync(finVisita))
                return false;

            return !await CheckConflictAsync(new ConflictCheckDto
            {
                AgenteId = agenteId,
                FechaHora = fechaHora,
                DuracionMinutos = duracionMinutos
            });
        }

        public async Task<bool> IsBusinessHourAsync(DateTime fechaHora)
        {
            if (fechaHora.DayOfWeek == DayOfWeek.Sunday)
                return false;

            if (fechaHora.DayOfWeek == DayOfWeek.Saturday && fechaHora.Hour >= 13)
                return false;

            var hora = fechaHora.TimeOfDay;
            var horaInicio = new TimeSpan(9, 0, 0);
            var horaFin = fechaHora.DayOfWeek == DayOfWeek.Saturday 
                ? new TimeSpan(13, 0, 0)
                : new TimeSpan(18, 0, 0);

            return hora >= horaInicio && hora < horaFin;
        }

        private VisitaResponseDto MapToResponseDto(Visita visita)
        {
            return new VisitaResponseDto
            {
                Id = visita.Id,
                PropiedadId = visita.PropiedadId,
                PropiedadCodigo = visita.Propiedad.Codigo,
                PropiedadDireccion = visita.Propiedad.Direccion,
                AgenteId = visita.AgenteId,
                AgenteNombre = visita.Agente.Nombre,
                ClienteNombre = visita.ClienteNombre,
                ClienteTelefono = visita.ClienteTelefono,
                ClienteEmail = visita.ClienteEmail,
                FechaHora = visita.FechaHora,
                DuracionMinutos = visita.DuracionMinutos,
                Observaciones = visita.Observaciones,
                Estado = visita.Estado,
                NotasVisita = visita.NotasVisita,
                FechaCreacion = visita.FechaCreacion,
                FechaActualizacion = visita.FechaActualizacion
            };
        }
    }
}