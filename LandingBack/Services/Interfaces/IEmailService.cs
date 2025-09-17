using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent, byte[]? attachment = null, string? attachmentName = null);
        Task SendVisitaConfirmationEmailAsync(int visitaId);
        Task SendVisitaReminderEmailAsync(int visitaId);
        Task SendVisitaCancellationEmailAsync(int visitaId, string motivo);
        Task SendVisitaRescheduleEmailAsync(int visitaId, DateTime fechaAnterior);
        Task<byte[]> GenerateICSAttachmentAsync(int visitaId);

        // Notificaciones de Leads
        Task SendLeadAssignmentEmailAsync(int leadId, int agenteId);
        Task SendLeadAssignmentEmailAsync(int leadId, int agenteId, int asignadoPorUsuarioId);
        Task SendLeadConfirmationEmailAsync(int leadId);
        Task SendLeadConfirmationEmailAsync(int leadId, string fromEmail, string fromName);

        // Método con emisor dinámico
        Task SendEmailWithCustomSenderAsync(string fromEmail, string fromName, string toEmail, string subject, string htmlContent, byte[]? attachment = null, string? attachmentName = null);
    }
}
