using CarRental.Core.Models;
using CarRental.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Web.Services
{
    public interface IVehicleSubResourceApiClient<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        Task<ApiResponse<List<TEntity>>> GetByVehicleIdAsync(int vehicleId);
        Task<ApiResponse> CreateAsync(int vehicleId, TDto dto);
        Task<ApiResponse> UpdateAsync(int entityId, TEntity entity);
        Task<ApiResponse> DeleteAsync(int entityId);

        // Métodos para reportes financieros
        Task<ApiResponse<List<TEntity>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int vehicleId = 0);
        Task<ApiResponse<List<TEntity>>> GetAllAsync();
    }


}