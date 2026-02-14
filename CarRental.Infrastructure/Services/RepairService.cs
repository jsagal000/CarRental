using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class RepairService : IRepairService
    {
        private readonly CarRentalDbContext _context;
        public RepairService(CarRentalDbContext context) { _context = context; }

        public async Task<Repair> AddRepairAsync(Repair repair)
        {
            _context.Repairs.Add(repair);
            await _context.SaveChangesAsync();
            return repair;
        }
        public async Task<Repair> GetRepairByIdAsync(int id) => await _context.Repairs.FindAsync(id);
        public async Task<IEnumerable<Repair>> GetRepairsByVehicleIdAsync(int vehicleId) => await _context.Repairs.Where(r => r.VehicleId == vehicleId).ToListAsync();
        public async Task UpdateRepairAsync(Repair repair)
        {
            _context.Entry(repair).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteRepairAsync(int id)
        {
            var repair = await _context.Repairs.FindAsync(id);
            if (repair != null)
            {
                _context.Repairs.Remove(repair);
                await _context.SaveChangesAsync();
            }
        }
    }
}