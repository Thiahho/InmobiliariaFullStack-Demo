// using Hangfire; // Commented out for now
using LandingBack.Data;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class VisitaJobService : IVisitaJobService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<VisitaJobService> _logger;

        public VisitaJobService(AppDbContext context, IEmailService emailService, ILogger<VisitaJobService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ScheduleVisitaReminderAsync(int visitaId)
        {
            try
            {
                var visita = await _context.Visitas.FindAsync(visitaId);
                if (visita == null)
                {
                    _logger.LogWarning($"Visita {visitaId} no encontrada para programar recordatorio");
                    return;
                }

                // TODO: Implementar con Hangfire más adelante
                // Por ahora solo logueamos que se debe programar
                var reminderTime = visita.FechaHora.AddHours(-24);
                
                if (reminderTime > DateTime.UtcNow)
                {
                    _logger.LogInformation($"Recordatorio pendiente para visita {visitaId} el {reminderTime}");
                    // En el futuro aquí se programaría con Hangfire
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error programando recordatorio para visita {visitaId}");
            }
        }

        public async Task CancelScheduledReminderAsync(int visitaId)
        {
            try
            {
                // TODO: Implementar cancelación con Hangfire más adelante
                _logger.LogInformation($"Recordatorio cancelado para visita {visitaId}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelando recordatorio para visita {visitaId}");
            }
        }

        public async Task ProcessRemindersAsync()
        {
            try
            {
                // Buscar visitas confirmadas que necesitan recordatorio en las próximas 24-25 horas
                var tomorrow = DateTime.UtcNow.AddHours(24);
                var dayAfterTomorrow = DateTime.UtcNow.AddHours(25);

                var visitasParaRecordatorio = await _context.Visitas
                    .Where(v => v.Estado == "Confirmada" && 
                               v.FechaHora >= tomorrow && 
                               v.FechaHora <= dayAfterTomorrow)
                    .ToListAsync();

                foreach (var visita in visitasParaRecordatorio)
                {
                    await SendReminderEmailAsync(visita.Id);
                }

                _logger.LogInformation($"Procesados {visitasParaRecordatorio.Count} recordatorios");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios automáticos");
            }
        }

        // [Queue("emails")] // Commented out for now
        public async Task SendReminderEmailAsync(int visitaId)
        {
            try
            {
                var visita = await _context.Visitas
                    .Include(v => v.Propiedad)
                    .Include(v => v.Agente)
                    .FirstOrDefaultAsync(v => v.Id == visitaId);

                if (visita == null)
                {
                    _logger.LogWarning($"Visita {visitaId} no encontrada para enviar recordatorio");
                    return;
                }

                // Solo enviar recordatorio si la visita está confirmada
                if (visita.Estado != "Confirmada")
                {
                    _logger.LogInformation($"Visita {visitaId} no está confirmada, saltando recordatorio");
                    return;
                }

                await _emailService.SendVisitaReminderEmailAsync(visitaId);
                _logger.LogInformation($"Recordatorio enviado para visita {visitaId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando recordatorio para visita {visitaId}");
                throw; // Re-lanzar para que Hangfire marque el job como fallido
            }
        }
    }
}
