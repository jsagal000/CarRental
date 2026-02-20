using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    /// <summary>
    /// Service that automatically updates rental statuses
    /// Call this service periodically or via a scheduled job
    /// </summary>
    public class RentalStatusUpdateService
    {
        private readonly CarRentalDbContext _context;
        private readonly ILogger<RentalStatusUpdateService> _logger;

        public RentalStatusUpdateService(
            CarRentalDbContext context,
            ILogger<RentalStatusUpdateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Updates all rental statuses based on current date
        /// </summary>
        public async Task<int> UpdateRentalStatusesAsync()
        {
            try
            {
                var today = DateTime.Today;
                var updatedCount = 0;

                _logger.LogInformation("Iniciando actualización de estados de alquileres...");

                // 1. Cambiar de Reservado a Activo (si la fecha de inicio es hoy o anterior)
                var reservedToActive = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Where(r => r.Status == Rental.RentalStatus.Reservado &&
                               r.StartDate.Date <= today)
                    .ToListAsync();

                foreach (var rental in reservedToActive)
                {
                    rental.Status = Rental.RentalStatus.Activo;

                    // Actualizar estado del vehículo a Alquilado
                    if (rental.Vehicle != null)
                    {
                        rental.Vehicle.State = Vehicle.VehicleState.Alquilado;
                    }

                    updatedCount++;
                    _logger.LogInformation($"Alquiler #{rental.Id} cambiado de Reservado → Activo");
                }

                // 2. Cambiar de Activo a Vencido (si la fecha de fin ya pasó)
                var activeToOverdue = await _context.Rentals
                    .Where(r => r.Status == Rental.RentalStatus.Activo &&
                               r.EndDate.Date < today &&
                               r.ActualReturnDate == null)
                    .ToListAsync();

                foreach (var rental in activeToOverdue)
                {
                    rental.Status = Rental.RentalStatus.Vencido;
                    updatedCount++;
                    _logger.LogInformation($"Alquiler #{rental.Id} cambiado de Activo → Vencido (vencido desde {rental.EndDate:yyyy-MM-dd})");
                }

                // Guardar todos los cambios
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✓ Actualización completada: {updatedCount} alquiler(es) actualizado(s)");
                }
                else
                {
                    _logger.LogInformation("✓ Actualización completada: No hay alquileres que actualizar");
                }

                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estados de alquileres");
                throw;
            }
        }
    }
}