namespace DrCell_V02.Data.Vistas
{
    public class VwVentaProducto
    {
    public int venta_item_id { get; set; }
    public int venta_id { get; set; }
    public DateTime? fecha_venta { get; set; }
    public string? estado { get; set; }
    public string? preference_id { get; set; }
    public string? payment_id { get; set; }
    public string? usuario_id { get; set; }
    public string? metodo_envio { get; set; }
    public string? direccion_envio { get; set; }
    public string? numero_seguimiento { get; set; }
    public decimal? venta_total { get; set; }
    public decimal? venta_costo_total { get; set; }
    public decimal? venta_margen_total { get; set; }

    public int variante_id { get; set; }
    public int cantidad { get; set; }
    public decimal precio_unitario { get; set; }
    public decimal linea_total { get; set; }

    public int producto_id { get; set; }
    public string? marca { get; set; }
    public string? modelo { get; set; }
    public string? categoria { get; set; }
    public int? categoria_id { get; set; }
    public bool producto_activo { get; set; }

    public string? color { get; set; }
    public string? ram { get; set; }
    public string? almacenamiento { get; set; }
    public decimal precio_lista_actual { get; set; }
    public int stock { get; set; }
    public bool variante_activa { get; set; }

    public decimal? proporcion_venta { get; set; }
    public decimal? costo_linea_estimado { get; set; }
    public decimal? margen_linea_estimado { get; set; }
    }
}
