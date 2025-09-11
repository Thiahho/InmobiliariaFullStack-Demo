using LandingBack.Data.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandingBack.Data.Configurations
{
    public class PropiedadHistorialConfiguration : IEntityTypeConfiguration<PropiedadHistorial>
    {
        public void Configure(EntityTypeBuilder<PropiedadHistorial> builder)
        {
            builder.HasKey(ph => ph.Id);
            
            builder.Property(ph => ph.Campo)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(ph => ph.ValorAnterior)
                .HasMaxLength(1000);
            
            builder.Property(ph => ph.ValorNuevo)
                .HasMaxLength(1000);
            
            builder.Property(ph => ph.FechaUtc)
                .IsRequired();
            
            // Configurar la relación con Propiedad
            builder.HasOne(ph => ph.Propiedad)
                .WithMany(p => p.Historial)
                .HasForeignKey(ph => ph.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}