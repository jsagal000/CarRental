using CarRental.Core.Models;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllAsync();
        Task<List<Permission>> GetByUserIdAsync(int userId);
        Task<List<Permission>> GetByRoleAsync(UserRole role);
        Task<Permission> GetByNameAsync(string name);
        Task<bool> HasPermissionAsync(int userId, string module, string action);
        Task<UserPermission> GetUserPermissionAsync(int userId, int permissionId);
        Task<UserPermission> AddUserPermissionAsync(UserPermission userPermission);
        Task UpdateUserPermissionAsync(UserPermission userPermission);
        Task RemoveUserPermissionAsync(int userId, int permissionId);
        Task CreateBulkAsync(List<Permission> permissions);
    }
}