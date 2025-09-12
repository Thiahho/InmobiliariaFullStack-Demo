using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("venta_item")]
    public class VentaItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("venta_id")]
    public int VentaId { get; set; }

    [Required]
    [Column("variante_id")]
    public int VarianteId { get; set; }

    [Required]
    [Column("cantidad")]
    public int Cantidad { get; set; }
    
    [Required][Column("precio_unitario")]
    public decimal PrecioUnitario { get; set; }
    
    [Required][Column("subtotal")]
    public decimal Subtotal { get; set; }
    
    // Navegación
    public Venta Venta { get; set; } = null!;
    public ProductosVariantes Variante { get; set; } = null!;
}
}