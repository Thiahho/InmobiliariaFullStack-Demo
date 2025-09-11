using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IVisitaService
    {
        // CRUD básico
        Task<VisitaResponseDto> GetVisitaByIdAsync(int id);
        Task<(IEnumerable<VisitaResponseDto> Visitas, int TotalCount)> GetVisitasAsync(VisitaSearchDto searchDto);
        Task<VisitaResponseDto> CreateVisitaAsync(VisitaCreateDto visitaCreateDto, int creadoPorUsuarioId);
        Task<VisitaResponseDto> UpdateVisitaAsync(int id, VisitaUpdateDto visitaUpdateDto, int usuarioId);
        Task DeleteVisitaAsync(int id);

        // Gestión de agenda
        Task<IEnumerable<VisitaResponseDto>> GetVisitasByAgenteAsync(int agenteId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<IEnumerable<VisitaResponseDto>> GetVisitasByPropiedadAsync(int propiedadId);
        Task<IEnumerable<VisitaCalendarDto>> GetVisitasCalendarAsync(int? agenteId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

        // Disponibilidad y conflictos
        Task<DisponibilidadDto> GetDisponibilidadAgenteAsync(int agenteId, DateTime fecha);
        Task<bool> CheckConflictAsync(ConflictCheckDto conflictDto);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int agenteId, DateTime fecha, int duracionMinutos = 60);

        // Estados y acciones
        Task<VisitaResponseDto> ConfirmarVisitaAsync(int visitaId, int usuarioId);
        Task<VisitaResponseDto> CancelarVisitaAsync(int visitaId, string motivo, int usuarioId);
        Task<VisitaResponseDto> ReagendarVisitaAsync(int visitaId, DateTime nuevaFecha, int usuarioId);
        Task<VisitaResponseDto> MarcarComoRealizadaAsync(int visitaId, string? notas, int usuarioId);

        // Acciones masivas
        Task<bool> BulkActionAsync(BulkVisitaActionDto bulkActionDto, int usuarioId);

        // Estadísticas y reportes
        Task<VisitaStatsDto> GetVisitaStatsAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? agenteId = null);

        // Notificaciones
        Task SendVisitaNotificationAsync(int visitaId, string tipoNotificacion = "Email");
        Task<string> GenerateICSFileAsync(int visitaId);

        // Validaciones
        Task<bool> IsValidTimeSlotAsync(int agenteId, DateTime fechaHora, int duracionMinutos);
        Task<bool> IsBusinessHourAsync(DateTime fechaHora);
    }
}