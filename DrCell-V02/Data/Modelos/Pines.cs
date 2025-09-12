using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("pines")]
    public class Pines
    {
        [Key]
        public int id { get; set; }
        [Column("marca")]
        public string? marca { get; set; }
        [Column("modelo")]
        public string? modelo { get; set; }
        [Column("costo")]
        public double? costo { get; set; }
        [Column("arreglo")]
        public decimal? arreglo { get; set; }
        [Column("placa")]
        public decimal? placa { get; set; }
        [Column("tipo")]
        public string? tipo { get; set; }
    }
}
