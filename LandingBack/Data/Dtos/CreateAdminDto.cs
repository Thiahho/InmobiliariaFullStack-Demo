using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class CreateAdminDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
    }
}