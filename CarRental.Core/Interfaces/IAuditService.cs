using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;

namespace CarRental.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(int userId, string module, string action, int? entityId = null,
            string entityName = null, string description = null, object oldValues = null,
            object newValues = null, string ipAddress = null, string userAgent = null,
            bool isSuccess = true, string errorMessage = null);

        Task<ServiceResult<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterDto filter);
        Task<ServiceResult<List<AuditLogDto>>> GetUserActivityAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ServiceResult<Dictionary<string, int>>> GetActivitySummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}