namespace LandingBack.Data.Dtos
{
    public class VisitaAuditLogDto
    {
        public int Id { get; set; }
        public int VisitaId { get; set; }
        public string Accion { get; set; } = null!;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = null!;
        public string? ValoresAnteriores { get; set; }
        public string? ValoresNuevos { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaHora { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}