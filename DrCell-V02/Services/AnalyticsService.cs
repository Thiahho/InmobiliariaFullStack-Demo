using AutoMapper;
using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DrCell_V02.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AnalyticsService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DashboardKpiDto> GenerarDashboardKpisAsync()
        {
            var metricas = await GetMetricasGeneralesAsync();
            var kpis = await CalcularKpisPrincipalesAsync();
            var tendencias = await AnalizarTendenciasAsync(30);
            var alertas = await GenerarAlertasInteligentesAsync();

            return new DashboardKpiDto
            {
                MetricasGenerales = metricas,
                KpisPrincipales = kpis,
                Tendencias = tendencias,
                Alertas = alertas,
                FechaGeneracion = DateTime.UtcNow
            };
        }

        public async Task<MetricasGeneralesDto> GetMetricasGeneralesAsync()
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Ventas
            var ventasHoy = await _context.Ventas
                .Where(v => v.FechaVenta.Date == hoy && v.Estado == "APPROVED")
                .SumAsync(v => v.MontoTotal);

            var ventasSemana = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioSemana && v.FechaVenta <= hoy.AddDays(1) && v.Estado == "APPROVED")
                .SumAsync(v => v.MontoTotal);

            var ventasMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "APPROVED")
                .SumAsync(v => v.MontoTotal);

            // Ganancias
            var gananciasHoy = await _context.Ventas
                .Where(v => v.FechaVenta.Date == hoy && v.Estado == "APPROVED")
                .SumAsync(v => v.Margen ?? 0);

            var gananciasSemana = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioSemana && v.FechaVenta <= hoy.AddDays(1) && v.Estado == "APPROVED")
                .SumAsync(v => v.Margen ?? 0);

            var gananciasMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "APPROVED")
                .SumAsync(v => v.Margen ?? 0);

            // Productos vendidos
            var productosHoy = await _context.VentaItems
                .Where(vi => vi.Venta.FechaVenta.Date == hoy && vi.Venta.Estado == "APPROVED")
                .SumAsync(vi => vi.Cantidad);

            var productosSemana = await _context.VentaItems
                .Where(vi => vi.Venta.FechaVenta >= inicioSemana && vi.Venta.FechaVenta <= hoy.AddDays(1) && vi.Venta.Estado == "APPROVED")
                .SumAsync(vi => vi.Cantidad);

            var productosMes = await _context.VentaItems
                .Where(vi => vi.Venta.FechaVenta >= inicioMes && vi.Venta.Estado == "APPROVED")
                .SumAsync(vi => vi.Cantidad);

            // Ventas pendientes
            var ventasPendientes = await _context.Ventas
                .CountAsync(v => v.Estado == "PENDIENTE");

            // Ticket promedio del mes
            var ventasDelMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes && v.Estado == "APPROVED")
                .ToListAsync();

            var ticketPromedio = ventasDelMes.Count > 0 ? ventasDelMes.Average(v => v.MontoTotal) : 0;

            return new MetricasGeneralesDto
            {
                VentasHoy = ventasHoy,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                GananciasHoy = gananciasHoy,
                GananciasSemana = gananciasSemana,
                GananciasMes = gananciasMes,
                ProductosVendidosHoy = productosHoy,
                ProductosVendidosSemana = productosSemana,
                ProductosVendidosMes = productosMes,
                VentasPendientes = ventasPendientes,
                TicketPromedioMes = ticketPromedio
            };
        }

        public async Task<List<KpiDto>> CalcularKpisPrincipalesAsync()
        {
            var mesActual = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
            var mesAnterior = mesActual.AddMonths(-1);

            // Datos del mes actual
            var ventasMesActual = await _context.Ventas
                .Where(v => v.FechaVenta >= mesActual && v.Estado == "APPROVED")
                .ToListAsync();

            // Datos del mes anterior
            var ventasMesAnterior = await _context.Ventas
                .Where(v => v.FechaVenta >= mesAnterior && v.FechaVenta < mesActual && v.Estado == "APPROVED")
                .ToListAsync();

            var kpis = new List<KpiDto>();

            // KPI 1: Ingresos mensuales
            var ingresoActual = ventasMesActual.Sum(v => v.MontoTotal);
            var ingresoAnterior = ventasMesAnterior.Sum(v => v.MontoTotal);
            var crecimientoIngresos = CalcularCrecimiento(ingresoAnterior, ingresoActual);

            kpis.Add(new KpiDto
            {
                Nombre = "Ingresos Mensuales",
                Valor = ingresoActual,
                ValorAnterior = ingresoAnterior,
                CambioPorcentual = crecimientoIngresos,
                Unidad = "$",
                Tendencia = DeterminarTendenciaKpi(crecimientoIngresos),
                Descripcion = "Total de ingresos del mes actual vs mes anterior",
                EsCritico = crecimientoIngresos < -20
            });

            // KPI 2: Número de transacciones
            var transaccionesActual = ventasMesActual.Count;
            var transaccionesAnterior = ventasMesAnterior.Count;
            var crecimientoTransacciones = CalcularCrecimiento(transaccionesAnterior, transaccionesActual);

            kpis.Add(new KpiDto
            {
                Nombre = "Transacciones",
                Valor = transaccionesActual,
                ValorAnterior = transaccionesAnterior,
                CambioPorcentual = crecimientoTransacciones,
                Unidad = "ventas",
                Tendencia = DeterminarTendenciaKpi(crecimientoTransacciones),
                Descripcion = "Número de ventas completadas",
                EsCritico = crecimientoTransacciones < -15
            });

            // KPI 3: Ticket promedio
            var ticketActual = ventasMesActual.Count > 0 ? ventasMesActual.Average(v => v.MontoTotal) : 0;
            var ticketAnterior = ventasMesAnterior.Count > 0 ? ventasMesAnterior.Average(v => v.MontoTotal) : 0;
            var crecimientoTicket = CalcularCrecimiento(ticketAnterior, ticketActual);

            kpis.Add(new KpiDto
            {
                Nombre = "Ticket Promedio",
                Valor = ticketActual,
                ValorAnterior = ticketAnterior,
                CambioPorcentual = crecimientoTicket,
                Unidad = "$",
                Tendencia = DeterminarTendenciaKpi(crecimientoTicket),
                Descripcion = "Valor promedio por transacción",
                EsCritico = crecimientoTicket < -10
            });

            // KPI 4: Margen de ganancia
            var margenActual = ventasMesActual.Count > 0 && ventasMesActual.Sum(v => v.MontoTotal) > 0
                ? (ventasMesActual.Sum(v => v.Margen ?? 0) / ventasMesActual.Sum(v => v.MontoTotal)) * 100
                : 0;

            var margenAnterior = ventasMesAnterior.Count > 0 && ventasMesAnterior.Sum(v => v.MontoTotal) > 0
                ? (ventasMesAnterior.Sum(v => v.Margen ?? 0) / ventasMesAnterior.Sum(v => v.MontoTotal)) * 100
                : 0;

            var cambioMargen = margenActual - margenAnterior;

            kpis.Add(new KpiDto
            {
                Nombre = "Margen de Ganancia",
                Valor = margenActual,
                ValorAnterior = margenAnterior,
                CambioPorcentual = cambioMargen,
                Unidad = "%",
                Tendencia = DeterminarTendenciaKpi(cambioMargen),
                Descripcion = "Porcentaje de margen de ganancia sobre ventas",
                EsCritico = margenActual < 15
            });

            // KPI 5: Tasa de conversión (ventas aprobadas vs todas)
            var ventasTotalesActual = await _context.Ventas
                .Where(v => v.FechaVenta >= mesActual)
                .CountAsync();

            var tasaConversionActual = ventasTotalesActual > 0 ? (decimal)transaccionesActual / ventasTotalesActual * 100 : 0;

            var ventasTotalesAnterior = await _context.Ventas
                .Where(v => v.FechaVenta >= mesAnterior && v.FechaVenta < mesActual)
                .CountAsync();

            var tasaConversionAnterior = ventasTotalesAnterior > 0 ? (decimal)transaccionesAnterior / ventasTotalesAnterior * 100 : 0;
            var cambioConversion = tasaConversionActual - tasaConversionAnterior;

            kpis.Add(new KpiDto
            {
                Nombre = "Tasa de Conversión",
                Valor = tasaConversionActual,
                ValorAnterior = tasaConversionAnterior,
                CambioPorcentual = cambioConversion,
                Unidad = "%",
                Tendencia = DeterminarTendenciaKpi(cambioConversion),
                Descripcion = "Porcentaje de ventas exitosas vs iniciadas",
                EsCritico = tasaConversionActual < 70
            });

            return kpis;
        }

        public async Task<List<TendenciaDto>> AnalizarTendenciasAsync(int diasAnalisis = 30)
        {
            var fechaInicio = DateTime.Today.AddDays(-diasAnalisis);
            var tendencias = new List<TendenciaDto>();

            // Tendencia de ventas diarias
            var ventasDiarias = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new PuntoTendenciaDto
                {
                    Fecha = g.Key,
                    Valor = g.Sum(v => v.MontoTotal),
                    Etiqueta = g.Key.ToString("dd/MM")
                })
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            if (ventasDiarias.Count > 1)
            {
                var tasaCrecimientoVentas = CalcularTasaCrecimiento(ventasDiarias.Select(v => v.Valor).ToList());
                tendencias.Add(new TendenciaDto
                {
                    Metrica = "Ventas Diarias",
                    Datos = ventasDiarias,
                    TasaCrecimiento = tasaCrecimientoVentas,
                    Direccion = DeterminarDireccionTendencia(tasaCrecimientoVentas),
                    DiasAnalizados = diasAnalisis
                });
            }

            // Tendencia de ganancias diarias
            var gananciasDiarias = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new PuntoTendenciaDto
                {
                    Fecha = g.Key,
                    Valor = g.Sum(v => v.Margen ?? 0),
                    Etiqueta = g.Key.ToString("dd/MM")
                })
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            if (gananciasDiarias.Count > 1)
            {
                var tasaCrecimientoGanancias = CalcularTasaCrecimiento(gananciasDiarias.Select(g => g.Valor).ToList());
                tendencias.Add(new TendenciaDto
                {
                    Metrica = "Ganancias Diarias",
                    Datos = gananciasDiarias,
                    TasaCrecimiento = tasaCrecimientoGanancias,
                    Direccion = DeterminarDireccionTendencia(tasaCrecimientoGanancias),
                    DiasAnalizados = diasAnalisis
                });
            }

            // Tendencia de productos vendidos
            var productosDiarios = await _context.VentaItems
                .Where(vi => vi.Venta.FechaVenta >= fechaInicio && vi.Venta.Estado == "APPROVED")
                .GroupBy(vi => vi.Venta.FechaVenta.Date)
                .Select(g => new PuntoTendenciaDto
                {
                    Fecha = g.Key,
                    Valor = g.Sum(vi => vi.Cantidad),
                    Etiqueta = g.Key.ToString("dd/MM")
                })
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            if (productosDiarios.Count > 1)
            {
                var tasaCrecimientoProductos = CalcularTasaCrecimiento(productosDiarios.Select(p => p.Valor).ToList());
                tendencias.Add(new TendenciaDto
                {
                    Metrica = "Productos Vendidos",
                    Datos = productosDiarios,
                    TasaCrecimiento = tasaCrecimientoProductos,
                    Direccion = DeterminarDireccionTendencia(tasaCrecimientoProductos),
                    DiasAnalizados = diasAnalisis
                });
            }

            return tendencias;
        }

        public async Task<ProyeccionVentasDto> ProyectarVentasAsync(int diasProyeccion = 30)
        {
            // Obtener datos históricos de los últimos 90 días
            var fechaInicio = DateTime.Today.AddDays(-90);
            var ventasHistoricas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Ventas = g.Sum(v => v.MontoTotal), Ganancias = g.Sum(v => v.Margen ?? 0) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            if (ventasHistoricas.Count < 7)
            {
                // Datos insuficientes para proyección confiable
                return new ProyeccionVentasDto
                {
                    FechaProyeccion = DateTime.Today.AddDays(diasProyeccion),
                    VentasProyectadas = 0,
                    GananciasProyectadas = 0,
                    MargenConfianza = 0,
                    MetodoProyeccion = "Datos insuficientes",
                    Escenarios = new List<EscenarioProyeccionDto>(),
                    FactoresInfluencia = new List<FactorInfluenciaDto>()
                };
            }

            // Cálculo de proyección usando promedio móvil ponderado
            var ultimosSieteDias = ventasHistoricas.TakeLast(7).ToList();
            var ultimosQuinceDias = ventasHistoricas.TakeLast(15).ToList();
            var ultimosTreintaDias = ventasHistoricas.TakeLast(30).ToList();

            var promedioSemanal = ultimosSieteDias.Average(x => x.Ventas);
            var promedioQuincenal = ultimosQuinceDias.Average(x => x.Ventas);
            var promedioMensual = ultimosTreintaDias.Average(x => x.Ventas);

            // Promedio ponderado (más peso a datos recientes)
            var ventasDiariaProyectada = (promedioSemanal * 0.5m) + (promedioQuincenal * 0.3m) + (promedioMensual * 0.2m);
            var gananciasDiariaProyectada = ultimosSieteDias.Average(x => x.Ganancias);

            var ventasProyectadas = ventasDiariaProyectada * diasProyeccion;
            var gananciasProyectadas = gananciasDiariaProyectada * diasProyeccion;

            // Calcular margen de confianza basado en la variabilidad
            var variabilidad = CalcularVariabilidad(ultimosTreintaDias.Select(x => x.Ventas).ToList());
            var margenConfianza = Math.Min(95, Math.Max(60, 100 - (variabilidad * 10)));

            return new ProyeccionVentasDto
            {
                FechaProyeccion = DateTime.Today.AddDays(diasProyeccion),
                VentasProyectadas = Math.Round(ventasProyectadas, 2),
                GananciasProyectadas = Math.Round(gananciasProyectadas, 2),
                MargenConfianza = Math.Round(margenConfianza, 1),
                MetodoProyeccion = "Promedio móvil ponderado",
                Escenarios = await GenerarEscenariosProyeccionAsync(diasProyeccion),
                FactoresInfluencia = GenerarFactoresInfluencia()
            };
        }

        public async Task<List<EscenarioProyeccionDto>> GenerarEscenariosProyeccionAsync(int diasProyeccion = 30)
        {
            var proyeccionBase = await ProyectarVentasAsync(diasProyeccion);
            var escenarios = new List<EscenarioProyeccionDto>();

            // Escenario optimista (+20%)
            escenarios.Add(new EscenarioProyeccionDto
            {
                Nombre = "Optimista",
                Probabilidad = 25,
                VentasProyectadas = proyeccionBase.VentasProyectadas * 1.2m,
                GananciasProyectadas = proyeccionBase.GananciasProyectadas * 1.2m,
                Descripcion = "Crecimiento sostenido con buenas condiciones de mercado"
            });

            // Escenario realista (base)
            escenarios.Add(new EscenarioProyeccionDto
            {
                Nombre = "Realista",
                Probabilidad = 50,
                VentasProyectadas = proyeccionBase.VentasProyectadas,
                GananciasProyectadas = proyeccionBase.GananciasProyectadas,
                Descripcion = "Continuación de tendencias actuales"
            });

            // Escenario pesimista (-15%)
            escenarios.Add(new EscenarioProyeccionDto
            {
                Nombre = "Pesimista",
                Probabilidad = 25,
                VentasProyectadas = proyeccionBase.VentasProyectadas * 0.85m,
                GananciasProyectadas = proyeccionBase.GananciasProyectadas * 0.85m,
                Descripcion = "Desaceleración del mercado o factores externos adversos"
            });

            return escenarios;
        }

        public async Task<List<AlertaDto>> GenerarAlertasInteligentesAsync()
        {
            var alertas = new List<AlertaDto>();

            // Alerta: Ventas por debajo del promedio
            var ventasHoy = await _context.Ventas
                .Where(v => v.FechaVenta.Date == DateTime.Today && v.Estado == "APPROVED")
                .SumAsync(v => v.MontoTotal);

            var promedioUltimos30Dias = await _context.Ventas
                .Where(v => v.FechaVenta >= DateTime.Today.AddDays(-30) && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => g.Sum(v => v.MontoTotal))
                .DefaultIfEmpty(0)
                .AverageAsync();

            if (ventasHoy < promedioUltimos30Dias * 0.7m)
            {
                alertas.Add(new AlertaDto
                {
                    Titulo = "Ventas Bajas Hoy",
                    Mensaje = $"Las ventas de hoy (${ventasHoy:N2}) están 30% por debajo del promedio diario (${promedioUltimos30Dias:N2})",
                    Tipo = TipoAlertaEnum.MargenBajo,
                    Nivel = NivelAlertaEnum.Advertencia,
                    Accion = "Revisar estrategias de marketing o promociones especiales",
                    Datos = new Dictionary<string, object>
                    {
                        { "ventasHoy", ventasHoy },
                        { "promedioDiario", promedioUltimos30Dias }
                    }
                });
            }

            // Alerta: Stock bajo en productos populares
            var productosStockBajo = await _context.ProductosVariantes
                .Where(pv => pv.Stock <= 5 && pv.Activa)
                .Include(pv => pv.Producto)
                .ToListAsync();

            if (productosStockBajo.Any())
            {
                alertas.Add(new AlertaDto
                {
                    Titulo = "Stock Bajo",
                    Mensaje = $"hay {productosStockBajo.Count} productos con stock bajo (≤5 unidades)",
                    Tipo = TipoAlertaEnum.ProductoNoRentable,
                    Nivel = NivelAlertaEnum.Advertencia,
                    Accion = "Reabastecer inventario de productos populares",
                    Datos = new Dictionary<string, object>
                    {
                        { "productos", productosStockBajo.Select(p => new { p.Id, Nombre = $"{p.Producto.Marca} {p.Producto.Modelo}", p.Stock }) }
                    }
                });
            }

            // Alerta: Ventas pendientes acumuladas
            var ventasPendientes = await _context.Ventas
                .CountAsync(v => v.Estado == "PENDIENTE" && v.FechaVenta <= DateTime.UtcNow.AddHours(-24));

            if (ventasPendientes > 5)
            {
                alertas.Add(new AlertaDto
                {
                    Titulo = "Ventas Pendientes",
                    Mensaje = $"Hay {ventasPendientes} ventas pendientes de más de 24 horas",
                    Tipo = TipoAlertaEnum.SinGanancia,
                    Nivel = ventasPendientes > 10 ? NivelAlertaEnum.Critica : NivelAlertaEnum.Advertencia,
                    Accion = "Revisar y procesar ventas pendientes",
                    Datos = new Dictionary<string, object>
                    {
                        { "cantidadPendientes", ventasPendientes }
                    }
                });
            }

            return alertas;
        }

        public async Task<AnalisisClientesDto> AnalisisClientesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            fechaInicio ??= DateTime.Today.AddMonths(-6);
            fechaFin ??= DateTime.Today;

            var ventasConCliente = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin 
                           && v.Estado == "APPROVED" && v.UsuarioId.HasValue)
                .ToListAsync();

            var totalClientes = ventasConCliente.Select(v => v.UsuarioId).Distinct().Count();

            // Analisis de clientes nuevos vs recurrentes
            var clientesConCompras = ventasConCliente
                .GroupBy(v => v.UsuarioId)
                .Select(g => new 
                {
                    ClienteId = g.Key,
                    NumeroCompras = g.Count(),
                    MontoTotal = g.Sum(v => v.MontoTotal),
                    PrimeraCompra = g.Min(v => v.FechaVenta),
                    UltimaCompra = g.Max(v => v.FechaVenta)
                })
                .ToList();

            var clientesNuevos = clientesConCompras.Count(c => c.PrimeraCompra >= fechaInicio);
            var clientesRecurrentes = clientesConCompras.Count(c => c.NumeroCompras > 1);

            var valorVidaPromedio = clientesConCompras.Count > 0 ? clientesConCompras.Average(c => c.MontoTotal) : 0;

            // Calcular tasa de retención (clientes que compraron en los últimos 30 días)
            var ultimosMes = DateTime.Today.AddDays(-30);
            var clientesActivosRecientes = ventasConCliente
                .Where(v => v.FechaVenta >= ultimosMes)
                .Select(v => v.UsuarioId)
                .Distinct()
                .Count();

            var tasaRetencion = totalClientes > 0 ? (decimal)clientesActivosRecientes / totalClientes * 100 : 0;

            return new AnalisisClientesDto
            {
                TotalClientes = totalClientes,
                ClientesNuevos = clientesNuevos,
                ClientesRecurrentes = clientesRecurrentes,
                TasaRetencion = Math.Round(tasaRetencion, 1),
                ValorVidaPromedio = Math.Round(valorVidaPromedio, 2),
                Segmentos = await SegmentarClientesAsync(),
                PatronesCompra = await AnalisisPatronesCompraAsync()
            };
        }

        public async Task<List<SegmentoClienteDto>> SegmentarClientesAsync()
        {
            var fechaLimite = DateTime.Today.AddMonths(-6);
            var clientesConVentas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaLimite && v.Estado == "APPROVED" && v.UsuarioId.HasValue)
                .GroupBy(v => v.UsuarioId)
                .Select(g => new 
                {
                    ClienteId = g.Key,
                    NumeroCompras = g.Count(),
                    MontoTotal = g.Sum(v => v.MontoTotal),
                    TicketPromedio = g.Average(v => v.MontoTotal)
                })
                .ToListAsync();

            var segmentos = new List<SegmentoClienteDto>();

            // Segmentación por valor y frecuencia
            var clientesVip = clientesConVentas.Where(c => c.MontoTotal > 1000 && c.NumeroCompras >= 3).ToList();
            var clientesRegulares = clientesConVentas.Where(c => c.NumeroCompras >= 2 && c.MontoTotal <= 1000).ToList();
            var clientesEsporadicos = clientesConVentas.Where(c => c.NumeroCompras == 1).ToList();

            var totalClientes = clientesConVentas.Count;

            if (clientesVip.Any())
            {
                segmentos.Add(new SegmentoClienteDto
                {
                    Nombre = "VIP",
                    Cantidad = clientesVip.Count,
                    PorcentajeTotal = totalClientes > 0 ? (decimal)clientesVip.Count / totalClientes * 100 : 0,
                    TicketPromedio = clientesVip.Average(c => c.TicketPromedio),
                    FrecuenciaCompra = (decimal)clientesVip.Average(c => c.NumeroCompras),
                    Caracteristicas = "Clientes de alto valor con compras frecuentes"
                });
            }

            if (clientesRegulares.Any())
            {
                segmentos.Add(new SegmentoClienteDto
                {
                    Nombre = "Regular",
                    Cantidad = clientesRegulares.Count,
                    PorcentajeTotal = totalClientes > 0 ? (decimal)clientesRegulares.Count / totalClientes * 100 : 0,
                    TicketPromedio = clientesRegulares.Average(c => c.TicketPromedio),
                    FrecuenciaCompra = (decimal)clientesRegulares.Average(c => c.NumeroCompras),
                    Caracteristicas = "Clientes con compras moderadas y cierta lealtad"
                });
            }

            if (clientesEsporadicos.Any())
            {
                segmentos.Add(new SegmentoClienteDto
                {
                    Nombre = "Esporádico",
                    Cantidad = clientesEsporadicos.Count,
                    PorcentajeTotal = totalClientes > 0 ? (decimal)clientesEsporadicos.Count / totalClientes * 100 : 0,
                    TicketPromedio = clientesEsporadicos.Average(c => c.TicketPromedio),
                    FrecuenciaCompra = 1,
                    Caracteristicas = "Clientes con una sola compra, oportunidad de fidelización"
                });
            }

            return segmentos;
        }

        public async Task<List<ComportamientoClienteDto>> AnalisisPatronesCompraAsync()
        {
            var fechaLimite = DateTime.Today.AddMonths(-3);
            
            // Análisis de horarios preferidos
            var ventasPorHora = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaLimite && v.Estado == "APPROVED")
                .ToListAsync();

            var patronesHorarios = ventasPorHora
                .GroupBy(v => v.FechaVenta.Hour)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key}:00h")
                .ToList();

            var horarioPreferido = patronesHorarios.FirstOrDefault() ?? "No determinado";

            // Productos más comprados
            var productosPopulares = await _context.VentaItems
                .Where(vi => vi.Venta.FechaVenta >= fechaLimite && vi.Venta.Estado == "APPROVED")
                .Include(vi => vi.Variante)
                    .ThenInclude(v => v.Producto)
                .GroupBy(vi => new { vi.Variante.Producto.Marca, vi.Variante.Producto.Modelo })
                .OrderByDescending(g => g.Sum(vi => vi.Cantidad))
                .Take(5)
                .Select(g => $"{g.Key.Marca} {g.Key.Modelo}")
                .ToListAsync();

            var patrones = new List<ComportamientoClienteDto>
            {
                new ComportamientoClienteDto
                {
                    Patron = "Horarios de Mayor Actividad",
                    Frecuencia = ventasPorHora.Count,
                    ValorPromedio = ventasPorHora.Count > 0 ? ventasPorHora.Average(v => v.MontoTotal) : 0,
                    ProductosPreferidos = productosPopulares,
                    HorarioPreferido = horarioPreferido
                }
            };

            return patrones;
        }

        // Continúa con los demás métodos de inventario, estacionalidad, etc...
        // (El código es muy extenso, estos son los métodos principales)

        public async Task<AnalisisInventarioDto> AnalisisInventarioAsync()
        {
            // Implementación básica del análisis de inventario
            var productos = await _context.ProductosVariantes
                .Include(pv => pv.Producto)
                .Where(pv => pv.Activa)
                .ToListAsync();

            var productosTotales = productos.Count;
            var productosStockBajo = productos.Count(p => p.Stock <= 5);
            var productosSinStock = productos.Count(p => p.Stock == 0);
            var valorInventario = productos.Sum(p => p.Precio * p.Stock);

            return new AnalisisInventarioDto
            {
                ProductosTotales = productosTotales,
                ProductosStockBajo = productosStockBajo,
                ProductosSinStock = productosSinStock,
                ValorInventario = valorInventario,
                RotacionPromedio = 0, // Requerirá cálculo más complejo
                ProductosLentaRotacion = new List<ProductoRotacionDto>(),
                ProductosAltaRotacion = new List<ProductoRotacionDto>(),
                Alertas = new List<AlertaInventarioDto>()
            };
        }

        public async Task<List<ProductoRotacionDto>> ProductosLentaRotacionAsync(int cantidad = 10)
        {
            // Obtener productos con poca rotación en los últimos 90 días
            var fechaLimite = DateTime.Today.AddDays(-90);
            
            var productosRotacion = await _context.ProductosVariantes
                .Include(pv => pv.Producto)
                .Where(pv => pv.Activa && pv.Stock > 0)
                .Select(pv => new 
                {
                    pv.Id,
                    Nombre = $"{pv.Producto.Marca} {pv.Producto.Modelo}",
                    pv.Stock,
                    pv.Precio,
                    VentasRecientes = _context.VentaItems
                        .Where(vi => vi.VarianteId == pv.Id && vi.Venta.FechaVenta >= fechaLimite && vi.Venta.Estado == "APPROVED")
                        .Sum(vi => vi.Cantidad)
                })
                .ToListAsync();

            return productosRotacion
                .Where(p => p.VentasRecientes < 5) // Menos de 5 vendidos en 90 días
                .OrderBy(p => p.VentasRecientes)
                .ThenByDescending(p => p.Stock)
                .Take(cantidad)
                .Select(p => new ProductoRotacionDto
                {
                    VarianteId = p.Id,
                    ProductoNombre = p.Nombre,
                    StockActual = p.Stock,
                    CantidadVendida = p.VentasRecientes,
                    DiasSinVentas = (DateTime.Today - fechaLimite).Days,
                    ValorInventario = p.Stock * p.Precio,
                    VelocidadRotacion = (int)(p.VentasRecientes / 90m * 30), // Proyección mensual
                    RotacionAnual = p.VentasRecientes / 90m * 365,
                    DiasPromedioPermanencia = p.VentasRecientes > 0 ? (int)(90 / p.VentasRecientes * 30) : 999,
                    Recomendacion = p.VentasRecientes < 2 ? "Considerar liquidación" : "Revisar estrategia de marketing"
                })
                .ToList();
        }

        public async Task<List<AlertaInventarioDto>> AlertasInventarioAsync()
        {
            var alertas = new List<AlertaInventarioDto>();
            
            // Stock bajo
            var productosStockBajo = await _context.ProductosVariantes
                .Include(pv => pv.Producto)
                .Where(pv => pv.Activa && pv.Stock <= 5 && pv.Stock > 0)
                .Take(20)
                .ToListAsync();

            foreach (var producto in productosStockBajo)
            {
                alertas.Add(new AlertaInventarioDto
                {
                    VarianteId = producto.Id,
                    ProductoNombre = $"{producto.Producto.Marca} {producto.Producto.Modelo}",
                    StockActual = producto.Stock,
                    StockMinimo = 5,
                    Tipo = TipoAlertaInventario.StockBajo,
                    Mensaje = $"Quedan {producto.Stock} unidades",
                    DiasStock = producto.Stock <= 2 ? 1 : 3
                });
            }

            // Sin stock
            var productosSinStock = await _context.ProductosVariantes
                .Include(pv => pv.Producto)
                .Where(pv => pv.Activa && pv.Stock == 0)
                .Take(10)
                .ToListAsync();

            foreach (var producto in productosSinStock)
            {
                alertas.Add(new AlertaInventarioDto
                {
                    VarianteId = producto.Id,
                    ProductoNombre = $"{producto.Producto.Marca} {producto.Producto.Modelo}",
                    StockActual = 0,
                    StockMinimo = 1,
                    Tipo = TipoAlertaInventario.SinStock,
                    Mensaje = "Producto agotado",
                    DiasStock = 0
                });
            }

            return alertas;
        }

        public async Task<List<MetricasTemporalesDto>> AnalisisTemporalAsync(DateTime fechaInicio, DateTime fechaFin, string agrupacion = "dia")
        {
            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin && v.Estado == "APPROVED")
                .ToListAsync();

            var metricas = new List<MetricasTemporalesDto>();

            switch (agrupacion.ToLower())
            {
                case "dia":
                    metricas = ventas
                        .GroupBy(v => v.FechaVenta.Date)
                        .Select(g => new MetricasTemporalesDto
                        {
                            Fecha = g.Key,
                            Ventas = g.Sum(v => v.MontoTotal),
                            Ganancias = g.Sum(v => v.Margen ?? 0),
                            Transacciones = g.Count(),
                            TicketPromedio = g.Average(v => v.MontoTotal),
                            MargenPromedio = g.Sum(v => v.MontoTotal) > 0 ? g.Sum(v => v.Margen ?? 0) / g.Sum(v => v.MontoTotal) * 100 : 0
                        })
                        .OrderBy(m => m.Fecha)
                        .ToList();
                    break;

                case "semana":
                    metricas = ventas
                        .GroupBy(v => GetWeekOfYear(v.FechaVenta))
                        .Select(g => new MetricasTemporalesDto
                        {
                            Fecha = g.Min(v => v.FechaVenta.Date),
                            Ventas = g.Sum(v => v.MontoTotal),
                            Ganancias = g.Sum(v => v.Margen ?? 0),
                            Transacciones = g.Count(),
                            TicketPromedio = g.Average(v => v.MontoTotal),
                            MargenPromedio = g.Sum(v => v.MontoTotal) > 0 ? g.Sum(v => v.Margen ?? 0) / g.Sum(v => v.MontoTotal) * 100 : 0
                        })
                        .OrderBy(m => m.Fecha)
                        .ToList();
                    break;

                case "mes":
                    metricas = ventas
                        .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                        .Select(g => new MetricasTemporalesDto
                        {
                            Fecha = new DateTime(g.Key.Year, g.Key.Month, 1),
                            Ventas = g.Sum(v => v.MontoTotal),
                            Ganancias = g.Sum(v => v.Margen ?? 0),
                            Transacciones = g.Count(),
                            TicketPromedio = g.Average(v => v.MontoTotal),
                            MargenPromedio = g.Sum(v => v.MontoTotal) > 0 ? g.Sum(v => v.Margen ?? 0) / g.Sum(v => v.MontoTotal) * 100 : 0
                        })
                        .OrderBy(m => m.Fecha)
                        .ToList();
                    break;
            }

            return metricas;
        }

        public async Task<AnalisisEstacionalidadDto> AnalisisEstacionalidadAsync(int mesesAnalisis = 12)
        {
            var fechaInicio = DateTime.Today.AddMonths(-mesesAnalisis);
            
            var ventasPorMes = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaInicio && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new 
                {
                    Mes = g.Key,
                    Ventas = g.Sum(v => v.MontoTotal),
                    Transacciones = g.Count()
                })
                .ToListAsync();

            var promedioVentas = ventasPorMes.Count > 0 ? ventasPorMes.Average(v => v.Ventas) : 0;
            
            var patronesEstacionales = ventasPorMes
                .Select(v => new PatronEstacionalDto
                {
                    Mes = v.Mes,
                    NombreMes = new DateTime(2024, v.Mes, 1).ToString("MMMM", new CultureInfo("es-ES")),
                    Ventas = v.Ventas,
                    IndiceEstacionalidad = promedioVentas > 0 ? v.Ventas / promedioVentas : 0,
                    NumeroTransacciones = v.Transacciones
                })
                .OrderBy(p => p.Mes)
                .ToList();

            return new AnalisisEstacionalidadDto
            {
                MesesAnalizados = mesesAnalisis,
                PromedioVentasMensual = promedioVentas,
                PatronesEstacionales = patronesEstacionales,
                MesMayorVenta = patronesEstacionales.OrderByDescending(p => p.Ventas).FirstOrDefault()?.NombreMes ?? "N/A",
                MesMenorVenta = patronesEstacionales.OrderBy(p => p.Ventas).FirstOrDefault()?.NombreMes ?? "N/A",
                VariacionEstacional = patronesEstacionales.Count > 0 ? 
                    patronesEstacionales.Max(p => p.Ventas) - patronesEstacionales.Min(p => p.Ventas) : 0
            };
        }

        public async Task<List<AlertaDto>> AlertasTendenciasNegativasAsync()
        {
            var alertas = new List<AlertaDto>();
            var fechaLimite = DateTime.Today.AddDays(-14);
            
            // Analizar tendencia de ventas últimas 2 semanas
            var ventasUltimos14Dias = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaLimite && v.Estado == "APPROVED")
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => g.Sum(v => v.MontoTotal))
                .ToListAsync();

            if (ventasUltimos14Dias.Count >= 7)
            {
                var primera = ventasUltimos14Dias.Take(7).Average();
                var segunda = ventasUltimos14Dias.Skip(7).Average();
                var tendencia = primera > 0 ? ((segunda - primera) / primera) * 100 : 0;

                if (tendencia < -15)
                {
                    alertas.Add(new AlertaDto
                    {
                        Titulo = "Tendencia Negativa en Ventas",
                        Mensaje = $"Las ventas han disminuido {Math.Abs(tendencia):F1}% en la última semana",
                        Tipo = TipoAlertaEnum.VentasBajas,
                        Nivel = NivelAlertaEnum.Advertencia,
                        Accion = "Implementar estrategias de marketing o promociones",
                        Datos = new Dictionary<string, object>
                        {
                            { "tendenciaPorcentual", tendencia },
                            { "ventasSemanaPasada", primera },
                            { "ventasSemanaActual", segunda }
                        }
                    });
                }
            }

            return alertas;
        }

        // Métodos auxiliares privados
        private int GetWeekOfYear(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        private decimal CalcularCrecimiento(decimal valorAnterior, decimal valorActual)
        {
            if (valorAnterior == 0) return valorActual > 0 ? 100 : 0;
            return ((valorActual - valorAnterior) / valorAnterior) * 100;
        }

        private TipoTendenciaKpi DeterminarTendenciaKpi(decimal cambio)
        {
            return cambio switch
            {
                > 5 => TipoTendenciaKpi.Subiendo,
                < -5 => TipoTendenciaKpi.Bajando,
                _ => TipoTendenciaKpi.Estable
            };
        }

        private TipoTendenciaEnum DeterminarDireccionTendencia(decimal tasaCrecimiento)
        {
            return tasaCrecimiento switch
            {
                > 2 => TipoTendenciaEnum.Creciente,
                < -2 => TipoTendenciaEnum.Decreciente,
                _ => TipoTendenciaEnum.Estable
            };
        }

        private decimal CalcularTasaCrecimiento(List<decimal> valores)
        {
            if (valores.Count < 2) return 0;

            var primerValor = valores.Take(valores.Count / 2).Average();
            var segundoValor = valores.Skip(valores.Count / 2).Average();

            return CalcularCrecimiento(primerValor, segundoValor);
        }

        private decimal CalcularVariabilidad(List<decimal> valores)
        {
            if (valores.Count < 2) return 0;

            var promedio = valores.Average();
            var varianza = valores.Sum(v => (decimal)Math.Pow((double)(v - promedio), 2)) / valores.Count;
            var desviacion = (decimal)Math.Sqrt((double)varianza);

            return promedio > 0 ? desviacion / promedio : 0;
        }

        private List<FactorInfluenciaDto> GenerarFactoresInfluencia()
        {
            return new List<FactorInfluenciaDto>
            {
                new FactorInfluenciaDto { Factor = "Tendencia histórica", Impacto = 0.4m, Descripcion = "Basado en ventas de últimos 30 días" },
                new FactorInfluenciaDto { Factor = "Estacionalidad", Impacto = 0.3m, Descripcion = "Patrones de temporada" },
                new FactorInfluenciaDto { Factor = "Stock disponible", Impacto = 0.2m, Descripcion = "Inventario actual de productos populares" },
                new FactorInfluenciaDto { Factor = "Condiciones económicas", Impacto = 0.1m, Descripcion = "Factores externos del mercado" }
            };
        }

    }
}