using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("celulares")]
    public class Celular
    {
        [Key]
        public int id { get; set; }
        [Column("marca")]
        public string? marca { get; set; }
        [Column("modelo")]
        public string? modelo { get; set; }
    }
}
