using AutoMapper;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Data.Dtos;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ✅ MAPEO PRINCIPAL: Productos -> ProductoDto
        CreateMap<Productos, ProductoDto>()
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => 
                src.Categoria != null ? src.Categoria.Nombre : null))
            .ForMember(dest => dest.Img, opt => opt.MapFrom(src => 
                src.Img != null ? Convert.ToBase64String(src.Img) : null))  // Convertir byte[] a base64
            .ForMember(dest => dest.Variantes, opt => opt.MapFrom(src => 
                src.Variantes.Where(v => v.Activa)))  // Solo variantes activas
            .ForMember(dest => dest.TotalVariantes, opt => opt.MapFrom(src => 
                src.Variantes.Count(v => v.Activa)))
            .ForMember(dest => dest.TieneStock, opt => opt.MapFrom(src => 
                src.Variantes.Any(v => v.Activa && (v.Stock - v.StockReservado) > 0)))
            .ForMember(dest => dest.Precio, opt => opt.Ignore())        // Se calcula en el DTO
            .ForMember(dest => dest.PrecioMinimo, opt => opt.Ignore())  // Se calcula después
            .ForMember(dest => dest.PrecioMaximo, opt => opt.Ignore()); // Se calcula después

        // ✅ MAPEO REVERSO: ProductoDto -> Productos (para crear/actualizar)
        CreateMap<ProductoDto, Productos>()
            .ForMember(dest => dest.Img, opt => opt.MapFrom(src => 
                ConvertBase64ToByteArray(src.Img)))  // Usar método seguro para conversión
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())     // No mapear la navegación
            .ForMember(dest => dest.Variantes, opt => opt.Ignore());    // No mapear la colección

        // ✅ MAPEO: ProductosVariantes -> VarianteDto
        CreateMap<ProductosVariantes, ProductosVariantesDto>()
            .ForMember(dest => dest.StockDisponible, opt => opt.MapFrom(src => 
                src.Stock - src.StockReservado));

        // ✅ MAPEO REVERSO: VarianteDto -> ProductosVariantes
        CreateMap<ProductosVariantesDto, ProductosVariantes>()
            .ForMember(dest => dest.StockDisponible, opt => opt.Ignore()) // Es calculada
            .ForMember(dest => dest.Producto, opt => opt.Ignore())        // No mapear navegación
            .ForMember(dest => dest.Reservas, opt => opt.Ignore())        // No mapear colección
            .ForMember(dest => dest.VentaItems, opt => opt.Ignore());      // No mapear colección

        // ✅ MAPEO: Categoria -> CategoriaDto
        CreateMap<Categorias, CategoriaDto>();

        // ✅ MAPEO REVERSO: CategoriaDto -> Categoria
        CreateMap<CategoriaDto, Categorias>()
            .ForMember(dest => dest.Productos, opt => opt.Ignore());     // No mapear colección

        // ✅ MAPEO: Venta -> VentaDto
        CreateMap<Venta, VentaDto>();

        // ✅ MAPEO REVERSO: VentaDto -> Venta
        CreateMap<VentaDto, Venta>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());         // No mapear colección

        // ✅ MAPEO: VentaItem -> VentaItemDto
        CreateMap<VentaItem, VentaItemDto>();

        // ✅ MAPEO REVERSO: VentaItemDto -> VentaItem
        CreateMap<VentaItemDto, VentaItem>()
            .ForMember(dest => dest.Venta, opt => opt.Ignore())          // No mapear navegación
            .ForMember(dest => dest.Variante, opt => opt.Ignore());      // No mapear navegación
    }

    // Método seguro para convertir base64 a byte array
    private static byte[]? ConvertBase64ToByteArray(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String) || string.IsNullOrWhiteSpace(base64String))
            return null;

        try
        {
            // Limpiar la cadena base64 (remover posibles prefijos de data URL)
            var cleanBase64 = base64String;
            if (cleanBase64.Contains(','))
            {
                cleanBase64 = cleanBase64.Split(',')[1];
            }

            // Validar que la cadena tenga longitud válida para base64
            if (cleanBase64.Length % 4 != 0)
                return null;

            return Convert.FromBase64String(cleanBase64);
        }
        catch (FormatException)
        {
            // Si hay error en la conversión, retornar null en lugar de fallar
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}