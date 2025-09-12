using DrCell_V02.Data.Modelos;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrCell_V02.Data.Dtos
{
    public class ProductosVariantesDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockReservado { get; set; }
        public int StockDisponible { get; set; } // Calculado por AutoMapper
        public string? Color { get; set; }
        public string? Ram { get; set; }
        public string? Almacenamiento { get; set; }
        public bool Activa { get; set; } = true;
        [JsonIgnore]
        public ProductoDto? Producto { get; set; }

    }
}