using LandingBack.Data;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Text;

namespace LandingBack.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(AppDbContext context, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, byte[]? attachment = null, string? attachmentName = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_configuration["Email:FromName"], _configuration["Email:FromAddress"]));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlContent
                };

                // Agregar archivo ICS si se proporciona
                if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                {
                    bodyBuilder.Attachments.Add(attachmentName, attachment, ContentType.Parse("text/calendar"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado exitosamente a {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando email a {toEmail}");
                throw;
            }
        }

        public async Task SendVisitaConfirmationEmailAsync(int visitaId)
        {
            var visita = await GetVisitaWithDetailsAsync(visitaId);
            if (visita == null) throw new KeyNotFoundException($"Visita {visitaId} no encontrada");

            var icsAttachment = await GenerateICSAttachmentAsync(visitaId);
            
            // Email al cliente
            if (!string.IsNullOrEmpty(visita.ClienteEmail))
            {
                var htmlContent = GenerateVisitaConfirmationHtml(visita, isForClient: true);
                await SendEmailAsync(
                    visita.ClienteEmail, 
                    $"Visita confirmada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                    htmlContent,
                    icsAttachment,
                    $"visita-{visita.Id}.ics"
                );
            }

            // Email al agente
            var agenteHtmlContent = GenerateVisitaConfirmationHtml(visita, isForClient: false);
            await SendEmailAsync(
                visita.Agente.Email,
                $"Visita confirmada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                agenteHtmlContent,
                icsAttachment,
                $"visita-{visita.Id}.ics"
            );
        }

        public async Task SendVisitaReminderEmailAsync(int visitaId)
        {
            var visita = await GetVisitaWithDetailsAsync(visitaId);
            if (visita == null) throw new KeyNotFoundException($"Visita {visitaId} no encontrada");

            var icsAttachment = await GenerateICSAttachmentAsync(visitaId);

            // Recordatorio al cliente
            if (!string.IsNullOrEmpty(visita.ClienteEmail))
            {
                var htmlContent = GenerateVisitaReminderHtml(visita, isForClient: true);
                await SendEmailAsync(
                    visita.ClienteEmail,
                    $"Recordatorio: Visita mañana - {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                    htmlContent
                );
            }

            // Recordatorio al agente
            var agenteHtmlContent = GenerateVisitaReminderHtml(visita, isForClient: false);
            await SendEmailAsync(
                visita.Agente.Email,
                $"Recordatorio: Visita mañana - {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                agenteHtmlContent
            );
        }

        public async Task SendVisitaCancellationEmailAsync(int visitaId, string motivo)
        {
            var visita = await GetVisitaWithDetailsAsync(visitaId);
            if (visita == null) throw new KeyNotFoundException($"Visita {visitaId} no encontrada");

            // Email al cliente
            if (!string.IsNullOrEmpty(visita.ClienteEmail))
            {
                var htmlContent = GenerateVisitaCancellationHtml(visita, motivo, isForClient: true);
                await SendEmailAsync(
                    visita.ClienteEmail,
                    $"Visita cancelada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                    htmlContent
                );
            }

            // Email al agente
            var agenteHtmlContent = GenerateVisitaCancellationHtml(visita, motivo, isForClient: false);
            await SendEmailAsync(
                visita.Agente.Email,
                $"Visita cancelada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                agenteHtmlContent
            );
        }

        public async Task SendVisitaRescheduleEmailAsync(int visitaId, DateTime fechaAnterior)
        {
            var visita = await GetVisitaWithDetailsAsync(visitaId);
            if (visita == null) throw new KeyNotFoundException($"Visita {visitaId} no encontrada");

            var icsAttachment = await GenerateICSAttachmentAsync(visitaId);

            // Email al cliente
            if (!string.IsNullOrEmpty(visita.ClienteEmail))
            {
                var htmlContent = GenerateVisitaRescheduleHtml(visita, fechaAnterior, isForClient: true);
                await SendEmailAsync(
                    visita.ClienteEmail,
                    $"Visita reprogramada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                    htmlContent,
                    icsAttachment,
                    $"visita-{visita.Id}.ics"
                );
            }

            // Email al agente
            var agenteHtmlContent = GenerateVisitaRescheduleHtml(visita, fechaAnterior, isForClient: false);
            await SendEmailAsync(
                visita.Agente.Email,
                $"Visita reprogramada: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}",
                agenteHtmlContent,
                icsAttachment,
                $"visita-{visita.Id}.ics"
            );
        }

        public async Task<byte[]> GenerateICSAttachmentAsync(int visitaId)
        {
            var visita = await GetVisitaWithDetailsAsync(visitaId);
            if (visita == null) throw new KeyNotFoundException($"Visita {visitaId} no encontrada");

            var icsContent = GenerateICSContent(visita);
            return Encoding.UTF8.GetBytes(icsContent);
        }

        private async Task<dynamic?> GetVisitaWithDetailsAsync(int visitaId)
        {
            return await _context.Visitas
                .Include(v => v.Propiedad)
                .Include(v => v.Agente)
                .Where(v => v.Id == visitaId)
                .Select(v => new
                {
                    v.Id,
                    v.FechaHora,
                    v.DuracionMinutos,
                    v.ClienteNombre,
                    v.ClienteEmail,
                    v.ClienteTelefono,
                    v.Observaciones,
                    v.Estado,
                    Propiedad = new
                    {
                        v.Propiedad.Codigo,
                        v.Propiedad.Barrio,
                        v.Propiedad.Direccion,
                        v.Propiedad.Tipo,
                        v.Propiedad.Precio,
                        v.Propiedad.Moneda
                    },
                    Agente = new
                    {
                        v.Agente.Nombre,
                        v.Agente.Email,
                        v.Agente.Telefono
                    }
                })
                .FirstOrDefaultAsync();
        }

        private string GenerateICSContent(dynamic visita)
        {
            var fechaInicio = ((DateTime)visita.FechaHora).ToUniversalTime();
            var fechaFin = fechaInicio.AddMinutes(visita.DuracionMinutos);

            var ics = new StringBuilder();
            ics.AppendLine("BEGIN:VCALENDAR");
            ics.AppendLine("VERSION:2.0");
            ics.AppendLine("PRODID:-//Inmobiliaria//Visitas//ES");
            ics.AppendLine("BEGIN:VEVENT");
            ics.AppendLine($"UID:visita-{visita.Id}@inmobiliaria.com");
            ics.AppendLine($"DTSTART:{fechaInicio:yyyyMMddTHHmmssZ}");
            ics.AppendLine($"DTEND:{fechaFin:yyyyMMddTHHmmssZ}");
            ics.AppendLine($"SUMMARY:Visita: {visita.Propiedad.Codigo} {visita.Propiedad.Barrio}");
            ics.AppendLine($"DESCRIPTION:Visita a propiedad {visita.Propiedad.Tipo} en {visita.Propiedad.Direccion}\\nCliente: {visita.ClienteNombre}\\nAgente: {visita.Agente.Nombre}");
            ics.AppendLine($"LOCATION:{visita.Propiedad.Direccion}, {visita.Propiedad.Barrio}");
            ics.AppendLine("STATUS:CONFIRMED");
            ics.AppendLine("END:VEVENT");
            ics.AppendLine("END:VCALENDAR");

            return ics.ToString();
        }

        private string GenerateVisitaConfirmationHtml(dynamic visita, bool isForClient)
        {
            var recipient = isForClient ? visita.ClienteNombre : visita.Agente.Nombre;
            var role = isForClient ? "cliente" : "agente";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Visita Confirmada</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #2c5aa0;"">✅ Visita Confirmada</h2>
        
        <p>Hola {recipient},</p>
        
        <p>Tu visita ha sido confirmada con los siguientes detalles:</p>
        
        <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3>Detalles de la Propiedad</h3>
            <p><strong>Código:</strong> {visita.Propiedad.Codigo}</p>
            <p><strong>Tipo:</strong> {visita.Propiedad.Tipo}</p>
            <p><strong>Ubicación:</strong> {visita.Propiedad.Direccion}, {visita.Propiedad.Barrio}</p>
            <p><strong>Precio:</strong> {visita.Propiedad.Moneda} {visita.Propiedad.Precio:N0}</p>
        </div>
        
        <div style=""background-color: #e8f4f8; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3>Detalles de la Visita</h3>
            <p><strong>Fecha y Hora:</strong> {((DateTime)visita.FechaHora):dddd, dd/MM/yyyy HH:mm}</p>
            <p><strong>Duración:</strong> {visita.DuracionMinutos} minutos</p>
            <p><strong>Cliente:</strong> {visita.ClienteNombre}</p>
            <p><strong>Agente:</strong> {visita.Agente.Nombre} - {visita.Agente.Telefono}</p>
        </div>
        
        {(isForClient ? $@"
        <div style=""background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;"">
            <p><strong>Recordatorio:</strong> Llegá 5 minutos antes de la hora pactada. Ante cualquier consulta, contactate con tu agente {visita.Agente.Nombre} al {visita.Agente.Telefono}.</p>
        </div>" : "")}
        
        <p>El archivo .ics adjunto te permitirá agregar esta visita a tu calendario.</p>
        
        <hr style=""margin: 30px 0;"">
        <p style=""font-size: 12px; color: #666;"">
            Este es un email automático. Por favor no responder a esta dirección.
        </p>
    </div>
</body>
</html>";
        }

        private string GenerateVisitaReminderHtml(dynamic visita, bool isForClient)
        {
            var recipient = isForClient ? visita.ClienteNombre : visita.Agente.Nombre;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Recordatorio de Visita</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #ff6b35;"">⏰ Recordatorio: Visita Mañana</h2>
        
        <p>Hola {recipient},</p>
        
        <p>Te recordamos que tienes una visita programada para mañana:</p>
        
        <div style=""background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ff6b35;"">
            <h3>Detalles de la Visita</h3>
            <p><strong>Propiedad:</strong> {visita.Propiedad.Codigo} - {visita.Propiedad.Barrio}</p>
            <p><strong>Dirección:</strong> {visita.Propiedad.Direccion}</p>
            <p><strong>Fecha y Hora:</strong> {((DateTime)visita.FechaHora):dddd, dd/MM/yyyy HH:mm}</p>
            <p><strong>Duración:</strong> {visita.DuracionMinutos} minutos</p>
            {(isForClient ? $"<p><strong>Agente:</strong> {visita.Agente.Nombre} - {visita.Agente.Telefono}</p>" : $"<p><strong>Cliente:</strong> {visita.ClienteNombre} - {visita.ClienteTelefono}</p>")}
        </div>
        
        {(isForClient ? @"
        <div style=""background-color: #d4edda; padding: 15px; border-radius: 8px; margin: 20px 0;"">
            <p><strong>Importante:</strong> Llegá 5 minutos antes. Si tenés algún inconveniente, contactate inmediatamente con tu agente.</p>
        </div>" : @"
        <div style=""background-color: #d4edda; padding: 15px; border-radius: 8px; margin: 20px 0;"">
            <p><strong>Recordatorio:</strong> Prepará la documentación de la propiedad y confirmá que tengas las llaves.</p>
        </div>")}
        
        <p>¡Nos vemos mañana!</p>
        
        <hr style=""margin: 30px 0;"">
        <p style=""font-size: 12px; color: #666;"">
            Este es un email automático. Por favor no responder a esta dirección.
        </p>
    </div>
</body>
</html>";
        }

        private string GenerateVisitaCancellationHtml(dynamic visita, string motivo, bool isForClient)
        {
            var recipient = isForClient ? visita.ClienteNombre : visita.Agente.Nombre;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Visita Cancelada</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #dc3545;"">❌ Visita Cancelada</h2>
        
        <p>Hola {recipient},</p>
        
        <p>Lamentamos informarte que la siguiente visita ha sido cancelada:</p>
        
        <div style=""background-color: #f8d7da; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc3545;"">
            <h3>Detalles de la Visita Cancelada</h3>
            <p><strong>Propiedad:</strong> {visita.Propiedad.Codigo} - {visita.Propiedad.Barrio}</p>
            <p><strong>Dirección:</strong> {visita.Propiedad.Direccion}</p>
            <p><strong>Fecha y Hora:</strong> {((DateTime)visita.FechaHora):dddd, dd/MM/yyyy HH:mm}</p>
            <p><strong>Motivo:</strong> {motivo}</p>
        </div>
        
        {(isForClient ? $@"
        <p>Disculpas por las molestias. Tu agente {visita.Agente.Nombre} se contactará contigo para reprogramar la visita.</p>
        <p>Teléfono del agente: {visita.Agente.Telefono}</p>" : @"
        <p>Por favor contactate con el cliente para reprogramar si es necesario.</p>")}
        
        <hr style=""margin: 30px 0;"">
        <p style=""font-size: 12px; color: #666;"">
            Este es un email automático. Por favor no responder a esta dirección.
        </p>
    </div>
</body>
</html>";
        }

        private string GenerateVisitaRescheduleHtml(dynamic visita, DateTime fechaAnterior, bool isForClient)
        {
            var recipient = isForClient ? visita.ClienteNombre : visita.Agente.Nombre;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Visita Reprogramada</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #28a745;"">🔄 Visita Reprogramada</h2>
        
        <p>Hola {recipient},</p>
        
        <p>Tu visita ha sido reprogramada con nueva fecha y hora:</p>
        
        <div style=""background-color: #d1ecf1; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3>Detalles de la Propiedad</h3>
            <p><strong>Código:</strong> {visita.Propiedad.Codigo}</p>
            <p><strong>Ubicación:</strong> {visita.Propiedad.Direccion}, {visita.Propiedad.Barrio}</p>
        </div>
        
        <div style=""background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3>Cambio de Horario</h3>
            <p><strong>Fecha anterior:</strong> <span style=""text-decoration: line-through; color: #666;"">{fechaAnterior:dddd, dd/MM/yyyy HH:mm}</span></p>
            <p><strong>Nueva fecha:</strong> <span style=""color: #28a745; font-weight: bold;"">{((DateTime)visita.FechaHora):dddd, dd/MM/yyyy HH:mm}</span></p>
            <p><strong>Duración:</strong> {visita.DuracionMinutos} minutos</p>
        </div>
        
        <p>El archivo .ics adjunto contiene la nueva información para tu calendario.</p>
        
        {(isForClient ? $"<p>Ante cualquier consulta, contactate con tu agente {visita.Agente.Nombre} al {visita.Agente.Telefono}.</p>" : "")}
        
        <hr style=""margin: 30px 0;"">
        <p style=""font-size: 12px; color: #666;"">
            Este es un email automático. Por favor no responder a esta dirección.
        </p>
    </div>
</body>
</html>";
        }
    }
}
