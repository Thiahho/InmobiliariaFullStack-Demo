using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LandingBack.Services
{
    public class VisitaAuditoriaService : IVisitaAuditoriaService
    {
        private readonly AppDbContext _context;

        public VisitaAuditoriaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(int visitaId, string accion, int usuarioId, string usuarioNombre, 
            object? valoresAnteriores = null, object? valoresNuevos = null, 
            string? observaciones = null, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new VisitaAuditLog
            {
                VisitaId = visitaId,
                Accion = accion,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                ValoresAnteriores = valoresAnteriores != null ? JsonSerializer.Serialize(valoresAnteriores) : null,
                ValoresNuevos = valoresNuevos != null ? JsonSerializer.Serialize(valoresNuevos) : null,
                Observaciones = observaciones,
                FechaHora = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.VisitaAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<VisitaAuditLogDto>> GetHistorialVisitaAsync(int visitaId)
        {
            var historial = await _context.VisitaAuditLogs
                .Where(a => a.VisitaId == visitaId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            return historial.Select(h => new VisitaAuditLogDto
            {
                Id = h.Id,
                VisitaId = h.VisitaId,
                Accion = h.Accion,
                UsuarioId = h.UsuarioId,
                UsuarioNombre = h.UsuarioNombre,
                ValoresAnteriores = h.ValoresAnteriores,
                ValoresNuevos = h.ValoresNuevos,
                Observaciones = h.Observaciones,
                FechaHora = h.FechaHora,
                IpAddress = h.IpAddress,
                UserAgent = h.UserAgent
            });
        }

        public async Task<IEnumerable<VisitaAuditLogDto>> GetHistorialUsuarioAsync(int usuarioId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.VisitaAuditLogs
                .Where(a => a.UsuarioId == usuarioId);

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHora <= fechaHasta.Value);

            var historial = await query
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            return historial.Select(h => new VisitaAuditLogDto
            {
                Id = h.Id,
                VisitaId = h.VisitaId,
                Accion = h.Accion,
                UsuarioId = h.UsuarioId,
                UsuarioNombre = h.UsuarioNombre,
                ValoresAnteriores = h.ValoresAnteriores,
                ValoresNuevos = h.ValoresNuevos,
                Observaciones = h.Observaciones,
                FechaHora = h.FechaHora,
                IpAddress = h.IpAddress,
                UserAgent = h.UserAgent
            });
        }
    }
}