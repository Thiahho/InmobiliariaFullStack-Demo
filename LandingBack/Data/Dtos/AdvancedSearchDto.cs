using System.ComponentModel.DataAnnotations;

namespace LandingBack.Data.Dtos
{
    public class AdvancedSearchDto
    {
        // Búsqueda por texto
        public string? TextoBusqueda { get; set; }
        public string? Codigo { get; set; }
        public string? Direccion { get; set; }

        // Filtros básicos
        public string? TipoPropiedad { get; set; }
        public string? TipoOperacion { get; set; }
        public string? Estado { get; set; }

        // Filtros de precio
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }

        // Filtros de características
        public int? AmbientesMin { get; set; }
        public int? AmbientesMax { get; set; }
        public int? DormitoriosMin { get; set; }
        public int? DormitoriosMax { get; set; }
        public int? BañosMin { get; set; }
        public int? BañosMax { get; set; }

        // Filtros de superficie
        public int? SuperficieCubiertaMin { get; set; }
        public int? SuperficieCubiertaMax { get; set; }
        public int? SuperficieTotalMin { get; set; }
        public int? SuperficieTotalMax { get; set; }

        // Filtros de antigüedad
        public int? AntiguedadMin { get; set; }
        public int? AntiguedadMax { get; set; }

        // Filtros geográficos
        public GeoSearchDto? FiltroGeografico { get; set; }

        // Filtros de amenities
        public List<string>? Amenities { get; set; }

        // Filtros adicionales
        public string? Orientacion { get; set; }
        public bool? DisponibilidadInmediata { get; set; }
        public bool? AceptaMascotas { get; set; }
        public bool? AceptaCredito { get; set; }

        // Filtros de fecha
        public DateTime? FechaPublicacionDesde { get; set; }
        public DateTime? FechaPublicacionHasta { get; set; }

        // Paginación y ordenamiento
        public string? OrderBy { get; set; } = "FechaCreacion";
        public bool OrderDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Incluir inactivas (solo para admin)
        public bool IncluirInactivas { get; set; } = false;
    }

    public class GeoSearchDto
    {
        // Búsqueda por radio
        public GeoPointDto? Centro { get; set; }
        public double? RadioKm { get; set; }

        // Búsqueda por polígono/zona
        public List<GeoPointDto>? Poligono { get; set; }

        // Búsqueda por ubicación específica
        public string? Barrio { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
    }

    public class GeoPointDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitud { get; set; }
    }

    public class AutocompleteRequestDto
    {
        [Required]
        [MinLength(2)]
        public string Query { get; set; } = null!;

        public string? Tipo { get; set; } // "direccion"|"barrio"|"codigo"|"general"

        [Range(1, 50)]
        public int Limit { get; set; } = 10;
    }

    public class AutocompleteResultDto
    {
        public string Valor { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int Coincidencias { get; set; }
    }

    public class SearchStatsDto
    {
        public int TotalResultados { get; set; }
        public decimal? PrecioPromedio { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public Dictionary<string, int> ResultadosPorTipo { get; set; } = new();
        public Dictionary<string, int> ResultadosPorBarrio { get; set; } = new();
        public Dictionary<string, int> ResultadosPorPrecio { get; set; } = new();
    }

    public class SavedSearchDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public string ParametrosBusqueda { get; set; } = null!; // JSON serializado
        public bool NotificacionesActivas { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaEjecucion { get; set; }
        public int? ResultadosUltimaEjecucion { get; set; }
    }
}