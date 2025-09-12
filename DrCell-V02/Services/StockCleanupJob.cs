using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace DrCell_V02.Services
{
    public class StockCleanupJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockCleanupJob> _logger;

        public StockCleanupJob(IServiceProvider serviceProvider, ILogger<StockCleanupJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                
                var liberadas = await stockService.LiberarReservasExpiradasAsync();
                
                if (liberadas > 0)
                {
                    _logger.LogInformation("Liberadas {count} reservas expiradas", liberadas);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en StockCleanupJob");
            }
            
            // Ejecutar cada 5 minutos
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
}