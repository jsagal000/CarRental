using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IAuditRepository
    {
        Task<AuditLog> AddAsync(AuditLog auditLog);
        Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogFilterDto filter);
        Task<List<AuditLog>> GetByUserIdAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetActivitySummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}