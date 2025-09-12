using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace DrCell_V02.Data.Modelos
{
    [Table("productos_variantes")]
    public class ProductosVariantes
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("producto_id")]
        public int ProductoId { get; set; }
        [Column("stock")]
        [Required]
        public int Stock { get; set; }
        [Column("color")]
        [Required]
        public required string Color { get; set; }
        [Column("ram")]
        [Required]
        public required string Ram { get; set; }
        [Column("almacenamiento")]
        [Required]
        public required string Almacenamiento { get; set; }
        [Column("precio")]
        [Required]
        public decimal Precio { get; set; }

        [Column("stock_reservado")]
        [Required]
        public int StockReservado { get; set; }

        [Column("activa")]
        public Boolean Activa { get; set; } = true;

        [NotMapped]
        public int StockDisponible => Stock - StockReservado;

        // ✅ PROPIEDADES DE NAVEGACIÓN - TODAS deben existir
        public Productos Producto { get; set; } = null!;
        public ICollection<StockReserva> Reservas { get; set; } = new List<StockReserva>();
        public ICollection<VentaItem> VentaItems { get; set; } = new List<VentaItem>();  // ✅ ESTA FALTABA
  


    }
}
