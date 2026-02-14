using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly CarRentalDbContext _context;
        public MaintenanceService(CarRentalDbContext context) { _context = context; }

        public async Task<Maintenance> AddMaintenanceAsync(Maintenance maintenance)
        {
            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();
            return maintenance;
        }
        public async Task<Maintenance> GetMaintenanceByIdAsync(int id) => await _context.Maintenances.FindAsync(id);
        public async Task<IEnumerable<Maintenance>> GetMaintenancesByVehicleIdAsync(int vehicleId) => await _context.Maintenances.Where(m => m.VehicleId == vehicleId).ToListAsync();
        public async Task UpdateMaintenanceAsync(Maintenance maintenance)
        {
            _context.Entry(maintenance).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteMaintenanceAsync(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance != null)
            {
                _context.Maintenances.Remove(maintenance);
                await _context.SaveChangesAsync();
            }
        }
    }
}