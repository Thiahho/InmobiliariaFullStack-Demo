using DrCell_V02.Data.Modelos;

namespace DrCell_V02.Services.Interface
{
    public interface IStockService
    {
        Task<bool> VerificarStockDisponibleAsync(int varianteId, int cantidad);
        Task<StockReserva> ReservarStockAsync(int varianteId, int cantidad, string sessionId, string? preferenceId = null);
        Task<bool> ConfirmarReservaAsync(string preferenceId);
        Task<bool> LiberarReservaAsync(int reservaId, string motivo = "");
        Task<bool> LiberarReservasPorSessionAsync(string sessionId);
        Task<int> LiberarReservasExpiradasAsync();
        Task<List<StockReserva>> ObtenerReservasActivasAsync();
        // Task<bool> SincronizarStockReservadoAsync();
    }
}
