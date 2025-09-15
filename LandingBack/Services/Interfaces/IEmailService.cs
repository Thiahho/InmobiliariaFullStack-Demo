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
    }
}
