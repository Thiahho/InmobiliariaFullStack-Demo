using LandingBack.Data.Dtos;

namespace LandingBack.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest, string ipAddress, string userAgent);
        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task LogoutAsync(int agenteId);
    }
}