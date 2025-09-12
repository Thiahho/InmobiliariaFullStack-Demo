using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace DrCell_V02.Services
{
    public class StockService : IStockService
    {   
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockService> _logger;

        public StockService(ApplicationDbContext applicationDbContext, IMapper mapper, ILogger<StockService> logger)
        {
            _context = applicationDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> ConfirmarReservaAsync(string preferenceId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== INICIANDO CONFIRMACIÓN DE RESERVAS ===");
                _logger.LogInformation("PreferenceId: {preferenceId}", preferenceId);

                var reservas = await _context.StockReserva
                    .Include(r => r.Variante)
                    .Where(r => r.PreferenceId == preferenceId && r.Estado == "PENDIENTE")
                    .ToListAsync();
                
                if (!reservas.Any()) 
                {
                    _logger.LogWarning("No se encontraron reservas pendientes para PreferenceId: {preferenceId}", preferenceId);
                    return false;
                }

                _logger.LogInformation("Encontradas {count} reservas pendientes", reservas.Count);

                foreach (var reserva in reservas)
                {
                    var stockAntes = reserva.Variante.Stock;
                    var stockReservadoAntes = reserva.Variante.StockReservado;

                    _logger.LogInformation("ANTES - VarianteId: {varianteId}, Stock: {stock}, StockReservado: {stockReservado}, Cantidad a confirmar: {cantidad}", 
                        reserva.VarianteId, stockAntes, stockReservadoAntes, reserva.Cantidad);

                    // ✅ CONFIRMAR = Descontar del stock real
                    reserva.Variante.Stock -= reserva.Cantidad;
                    reserva.Variante.StockReservado -= reserva.Cantidad;
                    reserva.Estado = "CONFIRMADO";
                    reserva.Observaciones = "Pago confirmado - Stock descontado";

                    _logger.LogInformation("DESPUÉS - VarianteId: {varianteId}, Stock: {stock}, StockReservado: {stockReservado}", 
                        reserva.VarianteId, reserva.Variante.Stock, reserva.Variante.StockReservado);
                }

                _logger.LogInformation("Guardando cambios en la base de datos...");
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Confirmando transacción...");
                await transaction.CommitAsync();
                
                _logger.LogInformation("✅ CONFIRMACIÓN EXITOSA - {count} reservas confirmadas para PreferenceId: {preferenceId}", reservas.Count, preferenceId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ ERROR al confirmar reservas para PreferenceId: {preferenceId}", preferenceId);
                throw;
            }
        }

        public async Task<bool> LiberarReservaAsync(int reservaId, string motivo = "")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reserva = await _context.StockReserva
                    .Include(r => r.Variante)
                    .FirstOrDefaultAsync(r => r.Id == reservaId && r.Estado == "PENDIENTE");
                
                if (reserva == null) 
                {
                    _logger.LogWarning("No se encontró reserva pendiente con ID: {reservaId}", reservaId);
                    return false;
                }

                // ❌ LIBERAR = Restaurar stock reservado (con validación)
                var nuevoStockReservado = Math.Max(0, reserva.Variante.StockReservado - reserva.Cantidad);
                
                _logger.LogInformation("Liberando reserva - VarianteId: {varianteId}, StockReservadoAntes: {antes}, Cantidad: {cantidad}, StockReservadoDespués: {despues}", 
                    reserva.VarianteId, reserva.Variante.StockReservado, reserva.Cantidad, nuevoStockReservado);
                
                reserva.Variante.StockReservado = nuevoStockReservado;
                reserva.Estado = "LIBERADO";
                reserva.FechaExpiracion = DateTime.UtcNow;
                reserva.Observaciones = string.IsNullOrEmpty(motivo) ? "Reserva liberada" : motivo;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Liberada reserva ID: {reservaId}, Motivo: {motivo}", reservaId, motivo);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al liberar reserva ID: {reservaId}", reservaId);
                throw;
            }
        }

        public async Task<int> LiberarReservasExpiradasAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservasExpiradas = await _context.StockReserva
                    .Include(r => r.Variante)
                    .Where(r => r.FechaExpiracion < DateTime.UtcNow && r.Estado == "PENDIENTE")
                    .ToListAsync();
                
                if (reservasExpiradas.Count == 0) return 0;

                foreach (var reserva in reservasExpiradas)
                {
                    // ⏰ EXPIRAR = Restaurar stock reservado (con validación)
                    var nuevoStockReservado = Math.Max(0, reserva.Variante.StockReservado - reserva.Cantidad);
                    
                    _logger.LogInformation("Expirando reserva - VarianteId: {varianteId}, StockReservadoAntes: {antes}, Cantidad: {cantidad}, StockReservadoDespués: {despues}", 
                        reserva.VarianteId, reserva.Variante.StockReservado, reserva.Cantidad, nuevoStockReservado);
                    
                    reserva.Variante.StockReservado = nuevoStockReservado;
                    reserva.Estado = "EXPIRADO";
                    reserva.Observaciones = "Reserva expirada automáticamente";
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Liberadas {count} reservas expiradas", reservasExpiradas.Count);
                return reservasExpiradas.Count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al liberar reservas expiradas");
                throw;
            }
        }

        public async Task<bool> LiberarReservasPorSessionAsync(string sessionId)
        {
            try
            {
                var reservas = await _context.StockReserva
                    .Where(r => r.SessionId == sessionId && r.Estado == "PENDIENTE")
                    .ToListAsync();
                    
                if (reservas.Count == 0) return false;

                foreach (var reserva in reservas)
                {
                    await LiberarReservaAsync(reserva.Id, $"Sesión {sessionId} finalizada");
                }
                
                _logger.LogInformation("Liberadas {count} reservas para sesión: {sessionId}", reservas.Count, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reservas por sesión: {sessionId}", sessionId);
                throw;
            }
        }

        public async Task<List<StockReserva>> ObtenerReservasActivasAsync()
        {
            return await _context.StockReserva
                .Include(r => r.Variante)
                .Where(r => r.Estado == "PENDIENTE" && r.FechaExpiracion > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<StockReserva> ReservarStockAsync(int varianteId, int cantidad, string sessionId, string? preferenceId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
        
            try
            {
                // Verificar stock disponible con lock
                var variante = await _context.ProductosVariantes
                    .FirstOrDefaultAsync(v => v.Id == varianteId);
                    
                if (variante == null)
                    throw new InvalidOperationException("Variante no encontrada");
                    
                if (variante.StockDisponible < cantidad)
                    throw new InvalidOperationException($"Stock insuficiente. Disponible: {variante.StockDisponible}, Solicitado: {cantidad}");

                // Crear reserva
                var reserva = new StockReserva
                {
                    VarianteId = varianteId,
                    Cantidad = cantidad,
                    SessionId = sessionId,
                    PreferenceId = preferenceId,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddMinutes(10),
                    Estado = "PENDIENTE",
                    Observaciones = "Reserva automática para pago"
                };

                _context.StockReserva.Add(reserva);
                
                // Actualizar stock reservado
                variante.StockReservado += cantidad;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Reserva creada - VarianteId: {varianteId}, Cantidad: {cantidad}, SessionId: {sessionId}", 
                    varianteId, cantidad, sessionId);
                
                return reserva;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al reservar stock - VarianteId: {varianteId}, Cantidad: {cantidad}", varianteId, cantidad);
                throw;
            }
        }

        /*public Task<bool> SincronizarStockReservadoAsync()
        {
            throw new NotImplementedException();
        }
        */
        public async Task<bool> VerificarStockDisponibleAsync(int varianteId, int cantidad)
        {
            try
            {
                _logger.LogInformation("=== VERIFICANDO STOCK DISPONIBLE ===");
                _logger.LogInformation("VarianteId: {varianteId}, Cantidad solicitada: {cantidad}", varianteId, cantidad);
                
                var variante = await _context.ProductosVariantes
                    .FirstOrDefaultAsync(v => v.Id == varianteId);
                
                if (variante == null) 
                {
                    _logger.LogWarning("❌ Variante no encontrada: {varianteId}", varianteId);
                    return false;
                }
            
                var stockTotal = variante.Stock;
                var stockReservado = variante.StockReservado;
                var stockDisponible = variante.StockDisponible;
                var disponible = stockDisponible >= cantidad;
                
                _logger.LogInformation("Stock Total: {stockTotal}", stockTotal);
                _logger.LogInformation("Stock Reservado: {stockReservado}", stockReservado);
                _logger.LogInformation("Stock Disponible (calculado): {stockDisponible}", stockDisponible);
                _logger.LogInformation("Cantidad solicitada: {cantidad}", cantidad);
                _logger.LogInformation("¿Suficiente stock? {disponible}", disponible ? "✅ SÍ" : "❌ NO");
                
                if (!disponible)
                {
                    _logger.LogWarning("❌ STOCK INSUFICIENTE - Necesario: {cantidad}, Disponible: {stockDisponible}", cantidad, stockDisponible);
                }
                
                return disponible;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR al verificar stock disponible - VarianteId: {varianteId}", varianteId);
                return false;
            }
        }
    }
}