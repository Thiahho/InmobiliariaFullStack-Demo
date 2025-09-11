using LandingBack.Services.Interfaces;
using System.Drawing;

namespace LandingBack.Services
{
    public class GeoService : IGeoService
    {
        public Point CreatePoint(double? latitud, double? longitud)
        {
            if (!latitud.HasValue || !longitud.HasValue)
                return new Point();

            // Point usa X para longitud, Y para latitud
            return new Point((int)(longitud.Value * 1000000), (int)(latitud.Value * 1000000));
        }

        public (double? Latitud, double? Longitud) GetCoordinates(Point? point)
        {
            if (!point.HasValue || (point.Value.X == 0 && point.Value.Y == 0))
                return (null, null);

            // Convertir de vuelta a decimales
            var latitud = point.Value.Y / 1000000.0;
            var longitud = point.Value.X / 1000000.0;

            return (latitud, longitud);
        }
    }
}