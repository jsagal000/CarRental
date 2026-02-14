// CarRental.Infrastructure/Interfaces/IPaymentRepository.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByRentalIdAsync(int rentalId);
        Task<decimal> GetTotalPaidByRentalIdAsync(int rentalId);
    }
}