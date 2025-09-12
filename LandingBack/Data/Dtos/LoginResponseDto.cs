namespace LandingBack.Data.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string token { get; set; } = null!; // Para compatibilidad con frontend
        public DateTime ExpiresAt { get; set; }
        public AgenteDto Agente { get; set; } = null!;
    }
}