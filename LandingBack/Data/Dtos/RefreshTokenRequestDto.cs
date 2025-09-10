using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string AccessToken { get; set; } = null!;
        
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}