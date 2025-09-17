using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface ILeadService
    {
        // CRUD básico
        Task<LeadResponseDto> GetLeadByIdAsync(int id);
        Task<(IEnumerable<LeadResponseDto> Leads, int TotalCount)> GetLeadsAsync(LeadSearchDto searchDto);
        Task<LeadResponseDto> CreateLeadAsync(LeadCreateDto leadCreateDto);
        Task<LeadResponseDto> UpdateLeadAsync(int id, LeadUpdateDto leadUpdateDto);
        Task DeleteLeadAsync(int id);

        // Gestión de leads
        Task<LeadResponseDto> AssignLeadAsync(LeadAssignDto assignDto, int usuarioId);
        Task<LeadResponseDto> UpdateLeadStatusAsync(LeadStatusUpdateDto statusDto, int usuarioId);
        Task<IEnumerable<LeadResponseDto>> GetLeadsByAgenteAsync(int agenteId);
        Task<IEnumerable<LeadResponseDto>> GetUnassignedLeadsAsync();

        // Estadísticas y reportes
        Task<LeadStatsDto> GetLeadStatsAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<IEnumerable<LeadResponseDto>> GetLeadsByPropiedadAsync(int propiedadId);
        
        // Acciones masivas
        Task<bool> BulkActionAsync(BulkLeadActionDto bulkActionDto, int usuarioId);
        
        // Auto-asignación
        Task<int?> GetNextAvailableAgenteAsync();
        Task AutoAssignLeadAsync(int leadId);

        // Validaciones
        Task<bool> IsDuplicateLeadAsync(string email, int propiedadId, TimeSpan timeWindow);
        Task<bool> IsValidPropiedadAsync(int propiedadId);

        // Generación de visitas
        Task<(LeadResponseDto Lead, int? VisitaId)> CreateLeadWithVisitaAsync(LeadCreateDto leadCreateDto);
    }
}