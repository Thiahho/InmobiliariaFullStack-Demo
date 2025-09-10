using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}