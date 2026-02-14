// CarRental.Infrastructure/Repositories/VehicleRepository.cs
using CarRental.Infrastructure.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Vehicles
                // Filtrar por estados en español
                .Where(v => v.State == Vehicle.VehicleState.Disponible || v.State == Vehicle.VehicleState.Mantenimiento)
                .Where(v => !_context.Rentals.Any(r =>
                    r.VehicleId == v.Id &&
                    // Filtrar por estados en español para RentalStatus
                    (r.Status == Rental.RentalStatus.Reservado || r.Status == Rental.RentalStatus.Activo) &&
                    ((startDate < r.EndDate && endDate > r.StartDate))
                ))
                .ToListAsync();
        }
    }
}
