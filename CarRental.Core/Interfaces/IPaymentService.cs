// CarRental.Core/Interfaces/IPaymentService.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetPaymentsByRentalIdAsync(int rentalId);
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<Payment> AddPaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
        Task<decimal> GetTotalPaidByRentalIdAsync(int rentalId);
        Task<decimal> GetRemainingBalanceAsync(int rentalId);
    }
}