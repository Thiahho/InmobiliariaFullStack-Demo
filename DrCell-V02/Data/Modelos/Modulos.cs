using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("modulos")]
    public class Modulos
    {
        [Key]
        public int id { get; set; }
        [Column("marca")]
        public string? marca { get; set; }
        [Column("modelo")]
        public string? modelo { get; set; }
        [Column("costo")]
        public decimal costo { get; set; }
        [Column("arreglo")]
        public decimal arreglo { get; set; }
        [Column("color")]
        public string? color { get; set; }
        [Column("marco")]
        public bool marco { get; set; }
        [Column("tipo")]
        public string? tipo { get; set; }
        [Column("version")]
        public string? version { get; set; }
    }
}
