using System.Net.Quic;
using AutoMapper;
using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DrCell_V02.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VentaService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VentaDto>> GetAllVentasAsync()
        {
            var variantes = await _context.Ventas
                .Include(v => v.Items) // Incluir los items de la venta
                .ThenInclude(vi => vi.Variante) // Incluir la variante de cada item
                .ToListAsync();

            return _mapper.Map<IEnumerable<VentaDto>>(variantes);
        }

        public async Task<VentaDto> GetVentaByIdAsync(int id)
        {
            var producto = await _context.Ventas.Include(v => v.Items)
                .ThenInclude(vi => vi.Variante)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (producto == null) return null;

            return new VentaDto
            {
                Id = producto.Id,
                PreferenceId = producto.PreferenceId,
                PaymentId = producto.PaymentId,
                MontoTotal = producto.MontoTotal,
                Estado = producto.Estado,
                FechaVenta = producto.FechaVenta,
                Items = _mapper.Map<List<VentaItemDto>>(producto.Items).ToList()
            };

        }

        public async Task<IEnumerable<VentasPorEstadoDto>> GetVentasPorEstadoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {

            var query = _context.Ventas.AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            var ventas = await query.ToListAsync();
            var totalVentas = ventas.Count;
            var montoTotal = ventas.Sum(v => v.MontoTotal);

            var ventasPorEstado = ventas
                .GroupBy(v => v.Estado)
                .Select(g => new VentasPorEstadoDto
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    MontoTotal = g.Sum(v => v.MontoTotal),
                    Porcentaje = totalVentas > 0 ? (decimal)g.Count() / totalVentas * 100 : 0
                })
                .OrderByDescending(v => v.Cantidad)
                .ToList();
            return ventasPorEstado;

        }

        public async Task<ComparativaPeriodoDto> GetComparativaPeriodoAsync(TipoPeriodo tipoPeriodo)
        {
            var (fechaInicioActual, fechaFinActual) = CalcularPeriodo(tipoPeriodo);
            var (fechaInicioAnterior, fechaFinAnterior) = CalcularPeriodoAnterior(tipoPeriodo, fechaInicioActual, fechaFinActual);

            var filtroActual = new FiltroEstadisticasDto
            {
                TipoPeriodo = TipoPeriodo.Personalizado,
                FechaInicio = fechaInicioActual,
                FechaFin = fechaFinActual
            };
            var filtroAnterior = new FiltroEstadisticasDto
            {
                TipoPeriodo = TipoPeriodo.Personalizado,
                FechaInicio = fechaInicioAnterior,
                FechaFin = fechaFinAnterior
            };

            var periodoActual = await GetEstadisticasPeriodoAsync(filtroActual);
            var periodoAnterior = await GetEstadisticasPeriodoAsync(filtroAnterior);

            return new ComparativaPeriodoDto
            {
                PeriodoActual = periodoActual,
                PeriodoAnterior = periodoAnterior,
                CrecimientoVentas = CalcularCrecimiento(periodoAnterior.TotalVentas, periodoActual.TotalVentas),
                CrecimientoMonto = CalcularCrecimiento(periodoAnterior.MontoTotalVendido, periodoActual.MontoTotalVendido),
                CrecimientoMargen = CalcularCrecimiento(periodoAnterior.MargenTotal ?? 0, periodoActual.MargenTotal ?? 0)
            };
        }

        public async Task<EstadisticasPeriodoDto> GetEstadisticasPeriodoAsync(FiltroEstadisticasDto filtro)
        {
            var (fechaInicio, fechaFin) = CalcularPeriodo(filtro.TipoPeriodo, filtro.FechaInicio, filtro.FechaFin);

            var query = _context.Ventas.AsQueryable()
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin);

            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                query = query.Where(v => v.Estado == filtro.Estado);
            }

            var ventas = await query.Include(v => v.Items).ThenInclude(vi => vi.Variante).ToListAsync();

            var totalProductosVendidos = ventas.SelectMany(v => v.Items).Sum(vi => vi.Cantidad);
            var ventasAprobadas = ventas.Count(v => v.Estado == "APPROVED");
            var ventasPendientes = ventas.Count(v => v.Estado == "PENDIENTE");
            var ventasRechazadas = ventas.Count(v => v.Estado == "RECHAZADO");
            
            return new EstadisticasPeriodoDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalVentas = ventas.Count,
                TotalProductosVendidos = totalProductosVendidos,
                MontoTotalVendido = ventas.Sum(v => v.MontoTotal),
                CostoTotalVendido = ventas.Sum(v => v.CostoTotal),
                MargenTotal = ventas.Sum(v => v.Margen),
                PorcentajeMargen = ventas.Sum(v => v.MontoTotal) > 0 ? (ventas.Sum(v => v.Margen) / ventas.Sum(v => v.MontoTotal)) * 100 : 0,
                TicketPromedio = ventas.Count > 0 ? ventas.Average(v => v.MontoTotal) : 0,
                VentasAprobadas = ventasAprobadas,
                VentasPendientes = ventasPendientes,
                VentasRechazadas = ventasRechazadas,
                PorcentajeAprobacion = ventas.Count > 0 ? (decimal)ventasAprobadas / ventas.Count * 100 : 0
            };
        }

        public async Task<IEnumerable<ResumenDiarioDto>> GetResumenDiarioAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = _context.Ventas
                            .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                            .Include(v => v.Items).ToListAsync();

            var resumenDiario = ventas
                .Result
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new ResumenDiarioDto
                {
                    Fecha = g.Key,
                    VentasDelDia = g.Count(),
                    MontoDelDia = g.Sum(v => v.MontoTotal),
                    CostoDelDia = g.Sum(v => v.CostoTotal),
                    MargenDelDia = g.Sum(v => v.Margen),
                    ProductosVendidos = g.SelectMany(v => v.Items).Sum(i => i.Cantidad),
                    TicketPromedio = g.Average(v => v.MontoTotal)
                })
                .OrderBy(r => r.Fecha)
                .ToList();
                
                return resumenDiario;
        }

        public async Task<IEnumerable<TopProductoDto>> GetTopProductosAsync(int cantidad = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.VentaItems.AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(vi => vi.Venta.FechaVenta >= fechaInicio.Value);
            }
            
        if (fechaFin.HasValue)
            {
                query = query.Where(vi => vi.Venta.FechaVenta <= fechaFin.Value);
            }

            var topProductos = await query
                .Where(vi => vi.Venta.Estado == "APPROVED")
                .Include(vi => vi.Variante)
                    .ThenInclude(v => v.Producto)
                .GroupBy(vi => new { vi.VarianteId, vi.Variante.Producto.Marca, vi.Variante.Producto.Modelo, vi.Variante.Color, vi.Variante.Ram, vi.Variante.Almacenamiento })
                .Select(g => new TopProductoDto
                {
                    VarianteId = g.Key.VarianteId,
                    ProductoNombre = $"{g.Key.Marca} {g.Key.Modelo}",
                    VarianteDescripcion = $"{g.Key.Color} - {g.Key.Ram} - {g.Key.Almacenamiento}",
                    CantidadVendida = g.Sum(vi => vi.Cantidad),
                    MontoTotal = g.Sum(vi => vi.Subtotal),
                    PrecioPromedio = g.Average(vi => vi.PrecioUnitario),
                    NumeroVentas = g.Count()
                })
                .OrderByDescending(tp => tp.CantidadVendida)
                .Take(cantidad)
                .ToListAsync();

            return topProductos;
        }

        private (DateTime fechaInicio, DateTime fechaFin) CalcularPeriodo(TipoPeriodo tipoPeriodo, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var hoy = DateTime.Today;

            return tipoPeriodo switch
            {
                TipoPeriodo.Hoy => (hoy, hoy.AddDays(1).AddMilliseconds(-1)),
                TipoPeriodo.Semana => (hoy.AddDays(-(int)hoy.DayOfWeek), hoy.AddDays(7 - (int)hoy.DayOfWeek).AddMilliseconds(-1)),
                TipoPeriodo.Mes => (new DateTime(hoy.Year, hoy.Month, 1), new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)).AddDays(1).AddMilliseconds(-1)),
                TipoPeriodo.Trimestre => CalcularTrimestre(hoy),
                TipoPeriodo.Año => (new DateTime(hoy.Year, 1, 1), new DateTime(hoy.Year, 12, 31).AddDays(1).AddMilliseconds(-1)),
                TipoPeriodo.Personalizado => (fechaInicio ?? hoy.AddMonths(-1), fechaFin ?? hoy),
                _ => (hoy.AddMonths(-1), hoy)
            };
        }

        private (DateTime fechaInicio, DateTime fechaFin) CalcularTrimestre(DateTime fecha)
        {
            var trimestre = (fecha.Month - 1) / 3 + 1;
            var inicioTrimestre = new DateTime(fecha.Year, (trimestre - 1) * 3 + 1, 1);
            var finTrimestre = inicioTrimestre.AddMonths(3).AddDays(-1);
            return (inicioTrimestre, finTrimestre.AddDays(1).AddMilliseconds(-1));
        }

        private (DateTime fechaInicio, DateTime fechaFin) CalcularPeriodoAnterior(TipoPeriodo tipoPeriodo, DateTime fechaInicioActual, DateTime fechaFinActual)
        {
            var duracion = fechaFinActual - fechaInicioActual;
            return (fechaInicioActual - duracion, fechaInicioActual.AddMilliseconds(-1));
        }

        private decimal CalcularCrecimiento(decimal valorAnterior, decimal valorActual)
        {
            if (valorAnterior == 0) return valorActual > 0 ? 100 : 0;
            return ((valorActual - valorAnterior) / valorAnterior) * 100;
        }

        // Métodos de auditoría
        public async Task<IEnumerable<VentaDto>> GetVentasConAuditoriaAsync(FiltroAuditoriaDto filtro)
        {
            var query = _context.Ventas.AsQueryable();

            if (filtro.FechaInicio.HasValue)
                query = query.Where(v => v.FechaVenta >= filtro.FechaInicio.Value);

            if (filtro.FechaFin.HasValue)
                query = query.Where(v => v.FechaVenta <= filtro.FechaFin.Value);

            if (!string.IsNullOrEmpty(filtro.Estado))
                query = query.Where(v => v.Estado == filtro.Estado);

            if (!string.IsNullOrEmpty(filtro.ModificadoPor))
                query = query.Where(v => v.ModificadoPor == filtro.ModificadoPor);

            if (!string.IsNullOrEmpty(filtro.UsuarioId) && int.TryParse(filtro.UsuarioId, out int usuarioId))
                query = query.Where(v => v.UsuarioId == usuarioId);

            if (filtro.SoloModificadas == true)
                query = query.Where(v => v.FechaModificacion.HasValue);

            var ventas = await query
                .Include(v => v.Items)
                .ThenInclude(vi => vi.Variante)
                .OrderByDescending(v => v.FechaModificacion ?? v.FechaVenta)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VentaDto>>(ventas);
        }

        public async Task<ResumenAuditoriaDto> GetResumenAuditoriaAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.Ventas.AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(v => v.FechaVenta >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(v => v.FechaVenta <= fechaFin.Value);

            var ventas = await query.ToListAsync();

            var ventasModificadas = ventas.Where(v => v.FechaModificacion.HasValue).ToList();
            var totalVentas = ventas.Count;

            var adminQueMasModifica = ventasModificadas
                .GroupBy(v => v.ModificadoPor)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            return new ResumenAuditoriaDto
            {
                TotalVentas = totalVentas,
                VentasModificadas = ventasModificadas.Count,
                VentasNoModificadas = totalVentas - ventasModificadas.Count,
                PorcentajeModificadas = totalVentas > 0 ? (decimal)ventasModificadas.Count / totalVentas * 100 : 0,
                AdminQueMasModifica = adminQueMasModifica,
                ModificacionesTotales = ventasModificadas.Count,
                UltimaModificacion = ventasModificadas.Max(v => v.FechaModificacion)
            };
        }

        public async Task<VentaDto?> ActualizarVentaAsync(int ventaId, ActualizarVentaDto actualizacion)
        {
            var venta = await _context.Ventas
                .Include(v => v.Items)
                .ThenInclude(vi => vi.Variante)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null) return null;

            var huboCambios = false;

            // Actualizar campos si se proporcionan
            if (!string.IsNullOrEmpty(actualizacion.Estado) && venta.Estado != actualizacion.Estado)
            {
                venta.Estado = actualizacion.Estado;
                huboCambios = true;
            }

            if (!string.IsNullOrEmpty(actualizacion.PaymentId) && venta.PaymentId != actualizacion.PaymentId)
            {
                venta.PaymentId = actualizacion.PaymentId;
                huboCambios = true;
            }

            if (actualizacion.Observaciones != null && venta.Observaciones != actualizacion.Observaciones)
            {
                venta.Observaciones = actualizacion.Observaciones;
                huboCambios = true;
            }

            if (!string.IsNullOrEmpty(actualizacion.MetodoEnvio) && venta.MetodoEnvio != actualizacion.MetodoEnvio)
            {
                venta.MetodoEnvio = actualizacion.MetodoEnvio;
                huboCambios = true;
            }

            if (!string.IsNullOrEmpty(actualizacion.DireccionEnvio) && venta.DireccionEnvio != actualizacion.DireccionEnvio)
            {
                venta.DireccionEnvio = actualizacion.DireccionEnvio;
                huboCambios = true;
            }

            if (!string.IsNullOrEmpty(actualizacion.NumeroSeguimiento) && venta.NumeroSeguimiento != actualizacion.NumeroSeguimiento)
            {
                venta.NumeroSeguimiento = actualizacion.NumeroSeguimiento;
                huboCambios = true;
            }

            // Si hubo cambios, actualizar auditoría
            if (huboCambios)
            {
                venta.FechaModificacion = DateTime.UtcNow;
                venta.ModificadoPor = actualizacion.ModificadoPor;
                
                if (!string.IsNullOrEmpty(actualizacion.MotivoModificacion))
                {
                    venta.Observaciones = string.IsNullOrEmpty(venta.Observaciones) 
                        ? $"Modificado: {actualizacion.MotivoModificacion}"
                        : $"{venta.Observaciones}; Modificado: {actualizacion.MotivoModificacion}";
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<VentaDto>(venta);
        }
    }
}
