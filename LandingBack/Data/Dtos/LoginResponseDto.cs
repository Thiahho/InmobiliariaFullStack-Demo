namespace LandingBack.Data.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public AgenteDto Agente { get; set; } = null!;
    }
}