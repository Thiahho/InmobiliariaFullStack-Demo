using System.Drawing;

namespace LandingBack.Services.Interfaces
{
    public interface IGeoService
    {
        Point CreatePoint(double? latitud, double? longitud);
        (double? Latitud, double? Longitud) GetCoordinates(Point? point);
    }
}