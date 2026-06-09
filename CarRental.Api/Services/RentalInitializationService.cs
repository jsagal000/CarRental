// CarRental.Api/Services/RentalInitializationService.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Services
{
    /// <summary>
    /// Servicio que se ejecuta al iniciar la aplicación para actualizar estados de alquileres vencidos
    /// </summary>
    public class RentalInitializationService
    {
        private readonly CarRentalDbContext _context;
        private readonly ILogger<RentalInitializationService> _logger;

        public RentalInitializationService(
            CarRentalDbContext context,
            ILogger<RentalInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta la actualización de estados de alquileres al iniciar la aplicación
        /// </summary>
        public async Task InitializeRentalStatusesAsync()
        {
            _logger.LogInformation("🚀 Iniciando servicio de actualización de estados de alquileres...");

            try
            {
                var today = DateTime.Today;

                // ============================================================================
                // ✅ LÓGICA 1: Cambiar Reservado → Activo (si la fecha de inicio es hoy)
                // ============================================================================
                var reservedRentalsToActivate = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Where(r => r.Status == Rental.RentalStatus.Reservado && r.StartDate.Date <= today)
                    .ToListAsync();

                if (reservedRentalsToActivate.Any())
                {
                    _logger.LogInformation("📌 Encontrados {Count} alquileres Reservados para activar", reservedRentalsToActivate.Count);

                    foreach (var rental in reservedRentalsToActivate)
                    {
                        rental.Status = Rental.RentalStatus.Activo;
                        if (rental.Vehicle != null)
                        {
                            rental.Vehicle.State = Vehicle.VehicleState.Alquilado;
                        }
                        _logger.LogInformation("✅ Alquiler {RentalId}: Reservado → Activo (Fecha inicio: {StartDate})",
                            rental.Id, rental.StartDate.Date);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("💾 Guardados {Count} alquileres: Reservado → Activo", reservedRentalsToActivate.Count);
                }

                // ============================================================================
                // ✅ LÓGICA 2: Cambiar Activo → Vencido (si la fecha de fin ya pasó)
                // ============================================================================
                var activeRentalsOverdue = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Where(r => r.Status == Rental.RentalStatus.Activo && r.EndDate.Date < today)
                    .ToListAsync();

                if (activeRentalsOverdue.Any())
                {
                    _logger.LogInformation("📌 Encontrados {Count} alquileres Activos que han vencido", activeRentalsOverdue.Count);

                    foreach (var rental in activeRentalsOverdue)
                    {
                        var daysOverdue = (today - rental.EndDate.Date).Days;
                        rental.Status = Rental.RentalStatus.Vencido;

                        // Mantener vehículo como Alquilado para indicar que necesita devolución
                        if (rental.Vehicle != null)
                        {
                            rental.Vehicle.State = Vehicle.VehicleState.Alquilado;
                        }

                        _logger.LogInformation(
                            "⚠️  Alquiler {RentalId}: Activo → Vencido ({DaysOverdue} días vencido. EndDate: {EndDate}, Hoy: {Today})",
                            rental.Id,
                            daysOverdue,
                            rental.EndDate.Date,
                            today
                        );
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("💾 Guardados {Count} alquileres: Activo → Vencido", activeRentalsOverdue.Count);
                }

                // ============================================================================
                // 📊 RESUMEN
                // ============================================================================
                var totalUpdated = (reservedRentalsToActivate?.Count ?? 0) + (activeRentalsOverdue?.Count ?? 0);

                if (totalUpdated > 0)
                {
                    _logger.LogInformation(
                        "✅ RESUMEN: Total de {TotalUpdated} alquileres actualizados en el inicio:\n" +
                        "   - Reservado → Activo: {Activated}\n" +
                        "   - Activo → Vencido: {Overdue}",
                        totalUpdated,
                        reservedRentalsToActivate?.Count ?? 0,
                        activeRentalsOverdue?.Count ?? 0
                    );
                }
                else
                {
                    _logger.LogInformation("ℹ️  No hay alquileres que actualizar en el inicio de la aplicación");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar estados de alquileres en el inicio de la aplicación");
                // No relanzar la excepción para no romper el startup de la aplicación
            }
        }
    }
}
