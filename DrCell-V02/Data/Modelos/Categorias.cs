using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Modelos
{
    [Table("Categorias")]
    public class Categorias
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public bool Activa { get; set; } = true;

        public ICollection<Productos> Productos { get; set; } = new List<Productos>();
    }
}