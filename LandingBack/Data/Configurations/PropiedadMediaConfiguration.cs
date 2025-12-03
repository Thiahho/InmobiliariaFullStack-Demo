using LandingBack.Data.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LandingBack.Data.Configurations
{
    public class PropiedadMediaConfiguration : IEntityTypeConfiguration<PropiedadMedia>
    {
        public void Configure(EntityTypeBuilder<PropiedadMedia> builder)
        {
            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.Url)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(pm => pm.Titulo)
                .HasMaxLength(200);

            builder.Property(pm => pm.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pm => pm.TipoArchivo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pm => pm.NombreArchivo)
                .HasMaxLength(500);

            builder.Property(pm => pm.ContentType)
                .HasMaxLength(100);

            // Configurar DatosArchivo como bytea (datos binarios) en PostgreSQL
            builder.Property(pm => pm.DatosArchivo)
                .HasColumnType("bytea");

            // Configurar TamanoBytes como bigint en PostgreSQL
            builder.Property(pm => pm.TamanoBytes)
                .HasColumnType("bigint");

            builder.Property(pm => pm.Orden)
                .HasDefaultValue(0);

            builder.Property(pm => pm.EsPrincipal)
                .HasDefaultValue(false);

            // Configurar índices para mejorar el rendimiento
            builder.HasIndex(pm => pm.PropiedadId);
            builder.HasIndex(pm => new { pm.PropiedadId, pm.Orden });
        }
    }
}
