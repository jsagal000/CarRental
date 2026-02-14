using CarRental.Core.Models;

namespace CarRental.Core.Interfaces
{
    public interface ICompanySettingsService
    {
        Task<ApiResult<CompanySettings>> GetCompanySettingsAsync();
        Task<ApiResult<CompanySettings>> UpdateCompanySettingsAsync(CompanySettings settings);
        Task<ApiResult<bool>> UploadLogoAsync(byte[] logoData);
    }
}