using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Vistas
{
    public class vCelularM
    {
        public int celularId { get; set; }
        public string? modelo { get; set; }
        public string? marca { get; set; }
        public string? color { get; set; }
        public bool marco { get; set; }
        public string? tipo { get; set; }
        public string? version { get; set; }
        public decimal? arreglomodulo { get; set; }
    }
}
