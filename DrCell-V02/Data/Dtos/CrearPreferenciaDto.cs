using System.ComponentModel.DataAnnotations;

namespace DrCell_V02.Data.Dtos
{
    public class CrearPreferenciaDto
    {
        [Required]
        public List<PreferenciaItemDto> Items { get; set; } = new List<PreferenciaItemDto>();
    }

    public class PreferenciaItemDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int VarianteId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Marca { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string Modelo { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Ram { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Almacenamiento { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = "";

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }
 
        public string Categoria { get; set; } ="";

    }
}
