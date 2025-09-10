using LandingBack.Data.Modelos;
using System.Security.Claims;

namespace LandingBack.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(Agente agente);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateRefreshToken(Agente agente, string refreshToken);
    }
}