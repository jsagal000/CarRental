// CarRental.Infrastructure/Repositories/RentalRepository.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Repositories
{
    public class RentalRepository : GenericRepository<Rental>, IRentalRepository
    {
        public RentalRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Rental>> GetAllRentalsWithDetailsAsync()
        {
            // Incluye las propiedades de navegación Vehicle y Customer para obtener sus detalles
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .ToListAsync();
        }

        public async Task<Rental> GetRentalWithDetailsByIdAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Rental>> GetUpcomingRentalsAsync(DateTime cutoffDate)
        {
            // Filtra las rentas que están en estado 'Reservado' y cuya fecha de inicio es después o igual a la fecha de corte.
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .Where(r => r.Status == Rental.RentalStatus.Reservado && r.StartDate >= cutoffDate)
                .OrderBy(r => r.StartDate) // Ordenar por fecha de inicio
                .ToListAsync();
        }
    }
}