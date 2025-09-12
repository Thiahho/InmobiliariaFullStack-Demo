using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Vistas;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
             : base(options) { }

        public DbSet<Celular> Celulares { get; set; }
        public DbSet<Modulos> Modulos { get; set; }
        public DbSet<Baterias> Baterias { get; set; }
        public DbSet<Pines> Pines { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<ProductosVariantes> ProductosVariantes { get; set; }
        public DbSet<StockReserva> StockReserva { get; set; }
        public DbSet<VentaItem> VentaItems { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        /// VISTAS
        public DbSet<vCelularesMBP> vCelularesMBP => Set<vCelularesMBP>();
        public DbSet<vCelularM> vCelularM => Set<vCelularM>();
        public DbSet<vCelularB> vCelularB => Set<vCelularB>();
        public DbSet<vCelularP> vCelularP => Set<vCelularP>();
        public DbSet<VwVentaProducto> VwVentaProducto => Set<VwVentaProducto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .ToTable("usuarios")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Celular>()
                .ToTable("celulares")
                .HasKey(e => e.id);

            modelBuilder.Entity<Modulos>()
                .ToTable("modulos")
                .HasKey(m => m.id);

            modelBuilder.Entity<Baterias>()
                .ToTable("baterias")
                .HasKey(b => b.id);

            modelBuilder.Entity<Pines>()
                .ToTable("pines")
                .HasKey(p => p.id);

            // ✅ CONFIGURACIÓN COMPLETA DE CATEGORIAS
            modelBuilder.Entity<Categorias>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("categorias");
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Activa).HasDefaultValue(true);

                // Índices3
                entity.HasIndex(e => e.Nombre).IsUnique();
                entity.HasIndex(e => e.Activa);
            });

            // ✅ CONFIGURACIÓN COMPLETA DE PRODUCTOS CON RELACIÓN A CATEGORIAS
            modelBuilder.Entity<Productos>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("productos");
                entity.Property(e => e.Marca).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Modelo).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CategoriaId).IsRequired(); // ✅ IMPORTANTE: FK requerida
                entity.Property(e => e.Img).HasMaxLength(255);
                entity.Property(e => e.Activo).HasDefaultValue(true);

                // ✅ RELACIÓN CON CATEGORIAS
                entity.HasOne(e => e.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar categoría si tiene productos

                // ✅ RELACIÓN CON VARIANTES (ya la tenías, pero la mejoro)
                entity.HasMany(e => e.Variantes)
                    .WithOne(v => v.Producto)
                    .HasForeignKey(v => v.ProductoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.CategoriaId);
                entity.HasIndex(e => new { e.Marca, e.Modelo });
                entity.HasIndex(e => e.Activo);
            });

            // ✅ CONFIGURACIÓN COMPLETA DE PRODUCTOS VARIANTES
            modelBuilder.Entity<ProductosVariantes>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("productos_variantes");
                entity.Property(e => e.Precio).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Ram).HasMaxLength(20);
                entity.Property(e => e.Almacenamiento).HasMaxLength(20);
                entity.Property(e => e.Stock).HasDefaultValue(0);
                entity.Property(e => e.StockReservado).HasDefaultValue(0);
                entity.Property(e => e.Activa).HasDefaultValue(true);

                // Relación con Productos (ya configurada arriba, pero por claridad)
                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.Variantes)
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.ProductoId);
                entity.HasIndex(e => e.Activa);
            });

            // ✅ CONFIGURACIÓN DE STOCK RESERVA (sin cambios, está bien)
            modelBuilder.Entity<StockReserva>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("stock_reserva");
                entity.Property(e => e.SessionId).HasMaxLength(128).IsRequired();
                entity.Property(e => e.PreferenceId).HasMaxLength(255);
                entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                // Relación con ProductosVariantes
                entity.HasOne(e => e.Variante)
                    .WithMany(v => v.Reservas)
                    .HasForeignKey(e => e.VarianteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimizar consultas
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.PreferenceId);
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaExpiracion);
            });

            // ✅ CONFIGURACIÓN DE VENTA (sin cambios, está bien)
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("ventas");
                entity.Property(e => e.PreferenceId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PaymentId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();
                entity.Property(e => e.MontoTotal).HasPrecision(18, 2);

                // Relación con VentaItem
                entity.HasMany(e => e.Items)
                    .WithOne(i => i.Venta)
                    .HasForeignKey(i => i.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.PreferenceId);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.FechaVenta);
            });

            // ✅ CONFIGURACIÓN DE VENTA ITEM (sin cambios, está bien)
            modelBuilder.Entity<VentaItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("venta_item");
                entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);

                // Relación con ProductosVariantes
                entity.HasOne(e => e.Variante)
                    .WithMany(v => v.VentaItems)
                    .HasForeignKey(e => e.VarianteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de las vistas (sin cambios)
            modelBuilder.Entity<vCelularesMBP>()
                .HasNoKey()
                .ToView("vcelularmbp");

            modelBuilder.Entity<vCelularM>()
                .HasNoKey()
                .ToView("vcelularm");

            modelBuilder.Entity<vCelularB>()
                .HasNoKey()
                .ToView("vcelularb");

            modelBuilder.Entity<vCelularP>()
                .HasNoKey()
                .ToView("vcelularp");
                
             modelBuilder.Entity<VwVentaProducto>(eb =>
            {
                eb.HasNoKey();                                  // keyless
                eb.ToView("vw_ventas_productos", "public");     // nombre y esquema de la vista
                eb.Metadata.SetIsTableExcludedFromMigrations(true); // que EF no intente crearla

                // Tipos explícitos opcionales si lo necesitas
                eb.Property(p => p.venta_total).HasColumnType("numeric");
                eb.Property(p => p.venta_costo_total).HasColumnType("numeric");
                eb.Property(p => p.venta_margen_total).HasColumnType("numeric");
                eb.Property(p => p.precio_unitario).HasColumnType("numeric");
                eb.Property(p => p.linea_total).HasColumnType("numeric");
                eb.Property(p => p.precio_lista_actual).HasColumnType("numeric");
                eb.Property(p => p.proporcion_venta).HasColumnType("numeric");
                eb.Property(p => p.costo_linea_estimado).HasColumnType("numeric");
                eb.Property(p => p.margen_linea_estimado).HasColumnType("numeric");
            });   
        }
    }
}