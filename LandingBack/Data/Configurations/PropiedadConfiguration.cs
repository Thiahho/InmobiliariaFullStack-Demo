using LandingBack.Data.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandingBack.Data.Configurations
{
    public class PropiedadConfiguration : IEntityTypeConfiguration<Propiedad>
    {
        public void Configure(EntityTypeBuilder<Propiedad> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(p => p.Tipo)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(p => p.Operacion)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(p => p.Barrio)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(p => p.Comuna)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(p => p.Direccion)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(p => p.Moneda)
                .IsRequired()
                .HasMaxLength(10);
            
            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(p => p.Expensas)
                .HasColumnType("decimal(18,2)");
            
            // Configurar la relación con PropiedadMedia
            builder.HasMany(p => p.Medias)
                .WithOne(pm => pm.Propiedad)
                .HasForeignKey(pm => pm.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configurar la relación con PropiedadHistorial
            builder.HasMany(p => p.Historial)
                .WithOne(ph => ph.Propiedad)
                .HasForeignKey(ph => ph.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}