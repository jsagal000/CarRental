// CarRental.Infrastructure/Repositories/PaymentRepository.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByRentalIdAsync(int rentalId)
        {
            return await _context.Payments
                .Where(p => p.RentalId == rentalId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidByRentalIdAsync(int rentalId)
        {
            var payments = await _context.Payments
                .Where(p => p.RentalId == rentalId)
                .ToListAsync();

            var total = payments.Sum(p => p.Amount);

            return total;
        }
    }
}