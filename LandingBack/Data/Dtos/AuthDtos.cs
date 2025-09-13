using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{

    public class CreateAgenteDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        [MaxLength(20)]
        public string? Telefono { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "Agente"; // Agente | Cargador
        public bool Activo { get; set; } = true;
    }

    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        [MaxLength(20)]
        public string? Telefono { get; set; }
        [MinLength(6)]
        public string? Password { get; set; }
    }
}
