using CarRental.Core.Models;

namespace CarRental.Core.Interfaces
{
    public interface IPdfGeneratorService
    {
        Task<byte[]> GenerateRentalContractAsync(Rental rental, CompanySettings companySettings, string contractNumber);
    }
}