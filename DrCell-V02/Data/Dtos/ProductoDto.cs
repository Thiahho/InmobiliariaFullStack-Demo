using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrCell_V02.Data.Dtos
{
    public class ProductoDto
    {
       public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public string? Img { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        
        // Precio calculado desde las variantes
        
        // Rango de precios si hay múltiples variantes
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        
        // Lista de variantes disponibles
        public List<ProductosVariantesDto> Variantes { get; set; } = new List<ProductosVariantesDto>();

        // Información adicional
        public int TotalVariantes { get; set; }
        public bool TieneStock { get; set; }
        // public int TotalVariantes => Variantes?.Count ?? 0;
        // public bool TieneStock => Variantes?.Any(v => v.StockDisponible > 0) ?? false;
        public decimal Precio 
        { 
            get 
            {
                if (Variantes == null || !Variantes.Any())
                    return 0m;
                    
                var variantesValidas = Variantes
                    .Where(v => v.Activa && v.Precio > 0)
                    .ToList();
                    
                if (!variantesValidas.Any())
                    return 0m;
                    
                return variantesValidas.Min(v => v.Precio);
            }
        }
         public void CalcularRangoPrecios()
        {
            if (Variantes?.Any() == true)
            {
                var variantesActivas = Variantes.Where(v => v.Activa && v.Precio > 0);
                if (variantesActivas.Any())
                {
                    PrecioMinimo = variantesActivas.Min(v => v.Precio);
                    PrecioMaximo = variantesActivas.Max(v => v.Precio);
                }
            }
        }
        public int GetTotalStock()
        {
            return Variantes?.Sum(v => v.Stock) ?? 0;
        }

         private decimal GetBasePrice()
        {
            // ✅ CORREGIDO: Manejar el caso donde no hay variantes
            if (Variantes == null || !Variantes.Any())
            {
                return 0; // O el valor que prefieras por defecto
            }
            
            // Obtener el precio mínimo de las variantes activas con stock
            var variantesDisponibles = Variantes.Where(v => v.Activa && v.StockDisponible > 0);
            
            if (!variantesDisponibles.Any())
            {
                // Si no hay variantes con stock, obtener el precio mínimo de todas las variantes activas
                var variantesActivas = Variantes.Where(v => v.Activa);
                return variantesActivas.Any() ? variantesActivas.Min(v => v.Precio) : 0;
            }
            
            return variantesDisponibles.Min(v => v.Precio);
        }

        // public void CalcularRangoPrecios()
        // {
        //     if (Variantes == null || !Variantes.Any())
        //     {
        //         PrecioMinimo = null;
        //         PrecioMaximo = null;
        //         return;
        //     }
            
        //     var variantesActivas = Variantes.Where(v => v.Activa).ToList();
            
        //     if (variantesActivas.Any())
        //     {
        //         PrecioMinimo = variantesActivas.Min(v => v.Precio);
        //         PrecioMaximo = variantesActivas.Max(v => v.Precio);
        //     }
        //     else
        //     {
        //         PrecioMinimo = null;
        //         PrecioMaximo = null;
        //     }
        // }
        // Propiedad calculada para compatibilidad

        public IEnumerable<string> GetAvailableRAM()
        {
            return Variantes?.Select(v => v.Ram)
                .Where(r => r != null)
                .Distinct()
                .OrderBy(r => r) ?? Enumerable.Empty<string>();
        }

        // Método para obtener los almacenamientos disponibles por RAM
        public IEnumerable<string> GetAvailableStorage(string ram)
        {
            return Variantes?.Where(v => v.Ram == ram)
                .Select(v => v.Almacenamiento)
                .Where(s => s != null)
                .Distinct()
                .OrderBy(s => s) ?? Enumerable.Empty<string>();
        }

        // Método para obtener los colores disponibles por RAM y almacenamiento
        public IEnumerable<string> GetAvailableColors(string ram, string storage)
        {
            return Variantes?.Where(v => v.Ram == ram && v.Almacenamiento == storage)
                .Select(v => v.Color)
                .Where(c => c != null)
                .Distinct()
                .OrderBy(c => c) ?? Enumerable.Empty<string>();
        }
    }
}