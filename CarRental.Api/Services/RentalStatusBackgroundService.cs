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

        public RentalStatusBackgroundService(IServiceProvider serviceProvider, ILogger<RentalStatusBackgroundService> _logger)
        {
            _serviceProvider = serviceProvider;
            this._logger = _logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateRentalStatuses();
            }
        }

        // ============================================================================
        // ✅ MÉTODO MEJORADO: Ahora actualiza AMBOS estados (Reservado→Activo y Activo→Vencido)
        // ============================================================================
        private async Task UpdateRentalStatuses()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

                var today = DateTime.Today;
                _logger.LogInformation("Iniciando actualización de estados de alquileres. Fecha: {Today}", today);

                // ============================================================================
                // ✅ LÓGICA 1: Cambiar Reservado → Activo (cuando llega la fecha de inicio)
                // ============================================================================
                var reservedRentalsToActivate = await context.Rentals
                    .Where(r => r.Status == Rental.RentalStatus.Reservado && r.StartDate.Date == today)
                    .ToListAsync();

                if (reservedRentalsToActivate.Any())
                {
                    _logger.LogInformation("Encontrados {Count} alquileres Reservados que deben activarse", reservedRentalsToActivate.Count);

                    // ✅ OPTIMIZACIÓN: Cargar TODOS los vehículos en UNA SOLA consulta
                    var vehicleIds = reservedRentalsToActivate.Select(r => r.VehicleId).ToList();
                    var vehicles = await context.Vehicles
                        .Where(v => vehicleIds.Contains(v.Id))
                        .ToDictionaryAsync(v => v.Id);

                    foreach (var rental in reservedRentalsToActivate)
                    {
                        // Cambiar de Reservado a Activo
                        rental.Status = Rental.RentalStatus.Activo;

                        // Actualizar el estado del vehículo
                        if (vehicles.TryGetValue(rental.VehicleId, out var vehicle))
                        {
                            vehicle.State = Vehicle.VehicleState.Alquilado;
                            _logger.LogInformation("Vehículo {VehicleId} cambiado a Alquilado", vehicle.Id);
                        }

                        _logger.LogInformation("Alquiler {RentalId} cambió de Reservado a Activo (Fecha inicio: {StartDate})",
                            rental.Id, rental.StartDate.Date);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("✅ Actualizados {Count} alquileres: Reservado → Activo", reservedRentalsToActivate.Count);
                }

                // ============================================================================
                // ✅ LÓGICA 2: Cambiar Activo → Vencido (cuando pasa la fecha de fin) - NUEVA
                // ============================================================================
                var activeRentalsOverdue = await context.Rentals
                    .Where(r => r.Status == Rental.RentalStatus.Activo && r.EndDate.Date < today)
                    .ToListAsync();

                if (activeRentalsOverdue.Any())
                {
                    _logger.LogInformation("Encontrados {Count} alquileres Activos que han vencido", activeRentalsOverdue.Count);

                    // ✅ OPTIMIZACIÓN: Cargar TODOS los vehículos en UNA SOLA consulta
                    var overdueVehicleIds = activeRentalsOverdue.Select(r => r.VehicleId).ToList();
                    var overdueVehicles = await context.Vehicles
                        .Where(v => overdueVehicleIds.Contains(v.Id))
                        .ToDictionaryAsync(v => v.Id);

                    foreach (var rental in activeRentalsOverdue)
                    {
                        var previousStatus = rental.Status;
                        rental.Status = Rental.RentalStatus.Vencido;

                        // NOTA: Mantener vehículo como "Alquilado" para indicar que necesita devolución
                        // Opción alternativa: cambiar a Disponible si prefieres que se pueda alquilar de nuevo
                        if (overdueVehicles.TryGetValue(rental.VehicleId, out var vehicle))
                        {
                            // ✅ RECOMENDADO: Mantener como Alquilado para avisar que necesita devolución
                            vehicle.State = Vehicle.VehicleState.Alquilado;
                            _logger.LogInformation("Vehículo {VehicleId} permanece como Alquilado (alquiler vencido)", vehicle.Id);
                        }

                        _logger.LogInformation(
                            "Alquiler {RentalId} cambió de {PreviousStatus} a Vencido (EndDate: {EndDate}, Hoy: {Today}, Días vencido: {DaysOverdue})",
                            rental.Id,
                            previousStatus,
                            rental.EndDate.Date,
                            today,
                            (today - rental.EndDate.Date).Days
                        );
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("✅ Actualizados {Count} alquileres: Activo → Vencido", activeRentalsOverdue.Count);
                }

                // ============================================================================
                // 📊 RESUMEN DE EJECUCIÓN
                // ============================================================================
                var totalUpdated = (reservedRentalsToActivate?.Count ?? 0) + (activeRentalsOverdue?.Count ?? 0);
                if (totalUpdated > 0)
                {
                    _logger.LogInformation(
                        "✅ Actualización completada. Total de alquileres procesados: {TotalUpdated} (Reservado→Activo: {Activated}, Activo→Vencido: {Overdue})",
                        totalUpdated,
                        reservedRentalsToActivate?.Count ?? 0,
                        activeRentalsOverdue?.Count ?? 0
                    );
                }
                else
                {
                    _logger.LogInformation("ℹ️  No hay alquileres que actualizar en este momento");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error actualizando estados de alquileres");
            }
        }
    }
}