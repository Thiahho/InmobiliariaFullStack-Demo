using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;

namespace LandingBack.Services.Interfaces
{
    public interface IVisitaAuditoriaService
    {
        Task LogActionAsync(int visitaId, string accion, int usuarioId, string usuarioNombre, 
            object? valoresAnteriores = null, object? valoresNuevos = null, 
            string? observaciones = null, string? ipAddress = null, string? userAgent = null);
        
        Task<IEnumerable<VisitaAuditLogDto>> GetHistorialVisitaAsync(int visitaId);
        Task<IEnumerable<VisitaAuditLogDto>> GetHistorialUsuarioAsync(int usuarioId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    }
}