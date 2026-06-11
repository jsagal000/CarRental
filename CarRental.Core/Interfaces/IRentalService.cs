// CarRental.Core/Interfaces/IRentalService.cs
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IRentalService
    {
        Task<IEnumerable<Rental>> GetAllRentalsAsync();
        Task<Rental> GetRentalByIdAsync(int id);
        Task<Rental> AddRentalAsync(Rental rental);
        Task UpdateRentalAsync(Rental rental);
        Task DeleteRentalAsync(int id);
        Task FinalizeRentalAsync(int rentalId, DateTime actualReturnDate);
        Task CancelRentalAsync(int rentalId, decimal cancellationAmount = 0);
        Task<decimal> CalculateRentalCostAsync(DateTime startDate, DateTime endDate, decimal dailyRate);

        Task<ApiResult<byte[]>> GenerateRentalContractPdfAsync(int rentalId);
    }
}
