namespace LandingBack.Data.Dtos
{
    public class AgenteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime UltimoLogin { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}