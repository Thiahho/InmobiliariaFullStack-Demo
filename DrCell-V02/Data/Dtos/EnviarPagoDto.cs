using System.ComponentModel.DataAnnotations;

namespace DrCell_V02.Data.Dtos
{
    public class EnviarPagoDto
    {
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        public int VarianteId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Surname { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string AreaCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string IdentificationType { get; set; } = "CC";
        
        [Required]
        [StringLength(50)]
        public string IdentificationNumber { get; set; } = string.Empty;
    }
}