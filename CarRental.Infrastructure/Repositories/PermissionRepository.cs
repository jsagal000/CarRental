using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly CarRentalDbContext _context;

        public PermissionRepository(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Module)
                .ThenBy(p => p.Action)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetByUserIdAsync(int userId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId && up.IsGranted)
                .Select(up => up.Permission)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetByRoleAsync(UserRole role)
        {
            return await _context.RolePermissions
                .Where(rp => rp.Role == role && rp.IsGranted)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<Permission> GetByNameAsync(string name)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == name && p.IsActive);
        }

        public async Task<bool> HasPermissionAsync(int userId, string module, string action)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return false;

            // Buscar el permiso
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Module == module && p.Action == action && p.IsActive);

            if (permission == null)
                return false;

            // 1. Verificar si hay permiso específico del usuario (tiene prioridad)
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.Id);

            if (userPermission != null)
            {
                return userPermission.IsGranted;
            }

            // 2. Verificar permiso del rol
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.Role == user.Role && rp.PermissionId == permission.Id);

            return rolePermission?.IsGranted ?? false;
        }

        public async Task<UserPermission> GetUserPermissionAsync(int userId, int permissionId)
        {
            return await _context.UserPermissions
                .Include(up => up.Permission)
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
        }

        public async Task<UserPermission> AddUserPermissionAsync(UserPermission userPermission)
        {
            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();
            return userPermission;
        }

        public async Task UpdateUserPermissionAsync(UserPermission userPermission)
        {
            _context.UserPermissions.Update(userPermission);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserPermissionAsync(int userId, int permissionId)
        {
            var userPermission = await GetUserPermissionAsync(userId, permissionId);
            if (userPermission != null)
            {
                _context.UserPermissions.Remove(userPermission);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates multiple permissions in a single transaction
        /// </summary>
        public async Task CreateBulkAsync(List<Permission> permissions)
        {
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();
        }
    }
}