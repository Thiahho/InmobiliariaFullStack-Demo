using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IUsuariosService
    {
        Task<IEnumerable<AgenteDto>> GetUsuariosAsync();
        Task<AgenteDto> CreateAgenteAsync(CreateAgenteDto dto, int adminId);
        Task<AgenteDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ToggleActivoAsync(int id);
    }
}


