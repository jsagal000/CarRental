using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;

namespace CarRental.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<ServiceResult<bool>> HasPermissionAsync(int userId, string module, string action);
        Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync();
        Task<ServiceResult<List<PermissionModuleDto>>> GetPermissionsGroupedByModuleAsync();
        Task<ServiceResult<UserPermissionDto>> GetUserPermissionsAsync(int userId);
        Task<ServiceResult<List<UserPermissionDto>>> GetAllUsersPermissionsAsync();
        Task<ServiceResult<bool>> UpdateUserPermissionsAsync(UpdateUserPermissionsDto updateDto);
        Task<ServiceResult<bool>> InitializeDefaultPermissionsAsync();
    }

}