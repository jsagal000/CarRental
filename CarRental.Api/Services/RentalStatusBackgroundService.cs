// CarRental.Api/Services/RentalStatusBackgroundService.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    public class RentalStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RentalStatusBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Ejecutar cada hora

        public RentalStatusBackgroundService(IServiceProvider serviceProvider, ILogger<RentalStatusBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateRentalStatuses();
            }
        }

        private async Task UpdateRentalStatuses()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

                var today = DateTime.Today;

                // Buscar rentals que necesitan cambio de estado
                var rentalsToUpdate = await context.Rentals
                    .Where(r => r.Status == Rental.RentalStatus.Reservado && r.StartDate.Date == today)
                    .ToListAsync();

                foreach (var rental in rentalsToUpdate)
                {
                    // Cambiar de Reservado a Activo
                    rental.Status = Rental.RentalStatus.Activo;

                    // Actualizar el estado del vehículo
                    var vehicle = await context.Vehicles.FindAsync(rental.VehicleId);
                    if (vehicle != null)
                    {
                        vehicle.State = Vehicle.VehicleState.Alquilado;
                    }

                    _logger.LogInformation("Rental {RentalId} status changed from Reservado to Activo", rental.Id);
                }

                if (rentalsToUpdate.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Updated {Count} rental statuses", rentalsToUpdate.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental statuses");
            }
        }
    }
}