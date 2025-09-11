using LandingBack.Data.Modelos;

namespace LandingBack.Services.Interfaces
{
    public interface IAuditoriaService
    {
        Task RegistrarCambioAsync(int propiedadId, string campo, string? valorAnterior, string? valorNuevo, int? usuarioId = null);
        Task RegistrarCambiosPropiedadAsync(Propiedad propiedadAnterior, Propiedad propiedadNueva, int? usuarioId = null);
    }
}