using LandingBack.Data;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(AppDbContext context, ILogger<AuditoriaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegistrarCambioAsync(int propiedadId, string campo, string? valorAnterior, string? valorNuevo, int? usuarioId = null)
        {
            try
            {
                if (valorAnterior == valorNuevo) return;

                var historial = new PropiedadHistorial
                {
                    PropiedadId = propiedadId,
                    Campo = campo,
                    ValorAnterior = valorAnterior,
                    ValorNuevo = valorNuevo,
                    UsuarioId = usuarioId,
                    FechaUtc = DateTime.UtcNow
                };

                _context.PropiedadHistoriales.Add(historial);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cambio registrado en propiedad {PropiedadId}: {Campo} = {ValorAnterior} -> {ValorNuevo}", 
                    propiedadId, campo, valorAnterior, valorNuevo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cambio en auditoría");
                throw;
            }
        }

        public async Task RegistrarCambiosPropiedadAsync(Propiedad propiedadAnterior, Propiedad propiedadNueva, int? usuarioId = null)
        {
            try
            {
                var cambios = new List<(string Campo, string? Anterior, string? Nuevo)>();

                // Campos críticos que necesitan auditoría
                if (propiedadAnterior.Precio != propiedadNueva.Precio)
                    cambios.Add(("Precio", propiedadAnterior.Precio.ToString(), propiedadNueva.Precio.ToString()));

                if (propiedadAnterior.Estado != propiedadNueva.Estado)
                    cambios.Add(("Estado", propiedadAnterior.Estado, propiedadNueva.Estado));

                if (propiedadAnterior.Destacado != propiedadNueva.Destacado)
                    cambios.Add(("Destacado", propiedadAnterior.Destacado.ToString(), propiedadNueva.Destacado.ToString()));

                if (propiedadAnterior.Expensas != propiedadNueva.Expensas)
                    cambios.Add(("Expensas", propiedadAnterior.Expensas?.ToString(), propiedadNueva.Expensas?.ToString()));

                if (propiedadAnterior.Tipo != propiedadNueva.Tipo)
                    cambios.Add(("Tipo", propiedadAnterior.Tipo, propiedadNueva.Tipo));

                if (propiedadAnterior.Operacion != propiedadNueva.Operacion)
                    cambios.Add(("Operacion", propiedadAnterior.Operacion, propiedadNueva.Operacion));

                if (propiedadAnterior.Barrio != propiedadNueva.Barrio)
                    cambios.Add(("Barrio", propiedadAnterior.Barrio, propiedadNueva.Barrio));

                if (propiedadAnterior.Comuna != propiedadNueva.Comuna)
                    cambios.Add(("Comuna", propiedadAnterior.Comuna, propiedadNueva.Comuna));

                if (propiedadAnterior.Direccion != propiedadNueva.Direccion)
                    cambios.Add(("Direccion", propiedadAnterior.Direccion, propiedadNueva.Direccion));

                // Registrar todos los cambios
                foreach (var (campo, anterior, nuevo) in cambios)
                {
                    await RegistrarCambioAsync(propiedadNueva.Id, campo, anterior, nuevo, usuarioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cambios masivos en auditoría");
                throw;
            }
        }
    }
}