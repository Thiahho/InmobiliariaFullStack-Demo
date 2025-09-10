using LandingBack.Data.Modelos;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agente> Agentes => Set<Agente>();
        public DbSet<Propiedad> Propiedades => Set<Propiedad>();
        public DbSet<PropiedadMedia> PropiedadMedias => Set<PropiedadMedia>();
        public DbSet<PropiedadHistorial> PropiedadHistoriales => Set<PropiedadHistorial>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Visita> Visitas => Set<Visita>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
