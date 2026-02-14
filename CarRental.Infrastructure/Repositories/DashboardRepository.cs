// CarRental.Infrastructure/Repositories/DashboardRepository.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly CarRentalDbContext _context;

        public DashboardRepository(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rental>> GetActiveRentalsAsync()
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .Where(r => r.Status == Rental.RentalStatus.Activo)
                .OrderBy(r => r.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.DateOfBirth.HasValue) // Solo clientes con fecha de nacimiento
                .ToListAsync();
        }

        public async Task<int> CountRentalsByStatusAsync(Rental.RentalStatus status)
        {
            return await _context.Rentals
                .CountAsync(r => r.Status == status);
        }

        public async Task<int> CountVehiclesByStateAsync(Vehicle.VehicleState state)
        {
            return await _context.Vehicles
                .CountAsync(v => v.State == state);
        }

        public async Task<int> CountAllCustomersAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesWithRentalsAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Partner)
                .Where(v => v.State == Vehicle.VehicleState.Alquilado || v.State == Vehicle.VehicleState.Disponible)
                .ToListAsync();
        }

        public async Task<Rental> GetActiveRentalForVehicle(int vehicleId)
        {
            return await _context.Rentals
                .Include(r => r.Customer)
                .Where(r => r.VehicleId == vehicleId && r.Status == Rental.RentalStatus.Activo)
                .FirstOrDefaultAsync();
        }
    }
}