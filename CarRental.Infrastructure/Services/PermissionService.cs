using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Interfaces;

namespace CarRental.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;

        public PermissionService(IPermissionRepository permissionRepository, IUserRepository userRepository)
        {
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
        }

        public async Task<ServiceResult<bool>> HasPermissionAsync(int userId, string module, string action)
        {
            try
            {
                // Primero verificar si tiene acceso al módulo
                var hasModuleAccess = await _permissionRepository.HasPermissionAsync(userId, module, "Access");
                if (!hasModuleAccess)
                    return ServiceResult<bool>.Success(false);

                // Luego verificar la acción específica
                var hasActionPermission = await _permissionRepository.HasPermissionAsync(userId, module, action);
                return ServiceResult<bool>.Success(hasActionPermission);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al verificar permisos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync()
        {
            try
            {
                var permissions = await _permissionRepository.GetAllAsync();
                var permissionDtos = permissions
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Module)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Module = p.Module,
                        Action = p.Action,
                        Type = p.Type,
                        ParentPermissionId = p.ParentPermissionId,
                        DisplayOrder = p.DisplayOrder,
                        IsActive = p.IsActive
                    }).ToList();

                return ServiceResult<List<PermissionDto>>.Success(permissionDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PermissionDto>>.Failure($"Error al obtener permisos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<PermissionModuleDto>>> GetPermissionsGroupedByModuleAsync()
        {
            try
            {
                var permissions = await _permissionRepository.GetAllAsync();

                var groupedPermissions = permissions
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Module)
                    .Select(g => new PermissionModuleDto
                    {
                        Module = g.Key,
                        ModuleName = GetModuleDisplayName(g.Key),
                        DisplayOrder = g.Min(p => p.DisplayOrder),
                        ModulePermission = g.FirstOrDefault(p => p.Type == PermissionType.Module),
                        Actions = g.Where(p => p.Type == PermissionType.Action)
                                   .OrderBy(p => p.DisplayOrder)
                                   .Select(p => new PermissionDto
                                   {
                                       Id = p.Id,
                                       Name = p.Name,
                                       Description = p.Description,
                                       Module = p.Module,
                                       Action = p.Action,
                                       Type = p.Type,
                                       ParentPermissionId = p.ParentPermissionId,
                                       DisplayOrder = p.DisplayOrder,
                                       IsActive = p.IsActive
                                   }).ToList()
                    })
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();

                return ServiceResult<List<PermissionModuleDto>>.Success(groupedPermissions);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PermissionModuleDto>>.Failure($"Error al obtener permisos agrupados: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserPermissionDto>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<UserPermissionDto>.Failure("Usuario no encontrado");

                var allPermissions = await _permissionRepository.GetAllAsync();
                var rolePermissions = await _permissionRepository.GetByRoleAsync(user.Role);
                var userPermissions = await _permissionRepository.GetByUserIdAsync(userId);

                var permissionStatuses = new List<PermissionStatus>();

                foreach (var permission in allPermissions.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder))
                {
                    var hasRolePermission = rolePermissions.Any(rp => rp.Id == permission.Id);
                    var userPermission = userPermissions.FirstOrDefault(up => up.Id == permission.Id);

                    bool isGranted;
                    bool isFromRole;
                    bool isDenied = false;

                    if (userPermission != null)
                    {
                        // Usuario tiene permiso específico (override)
                        var userPermRecord = await _permissionRepository.GetUserPermissionAsync(userId, permission.Id);
                        isGranted = userPermRecord?.IsGranted ?? false;
                        isFromRole = false;
                        isDenied = userPermRecord != null && !userPermRecord.IsGranted;
                    }
                    else if (hasRolePermission)
                    {
                        // Permiso viene del rol
                        isGranted = true;
                        isFromRole = true;
                    }
                    else
                    {
                        // No tiene el permiso
                        isGranted = false;
                        isFromRole = false;
                    }

                    permissionStatuses.Add(new PermissionStatus
                    {
                        PermissionId = permission.Id,
                        PermissionName = permission.Name,
                        Module = permission.Module,
                        Action = permission.Action,
                        Type = permission.Type,
                        ParentPermissionId = permission.ParentPermissionId,
                        DisplayOrder = permission.DisplayOrder,
                        IsGranted = isGranted,
                        IsFromRole = isFromRole,
                        IsDenied = isDenied
                    });
                }

                var userPermissionDto = new UserPermissionDto
                {
                    UserId = user.Id,
                    UserName = user.Username,
                    Role = user.Role,
                    Permissions = permissionStatuses
                };

                return ServiceResult<UserPermissionDto>.Success(userPermissionDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserPermissionDto>.Failure($"Error al obtener permisos del usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<UserPermissionDto>>> GetAllUsersPermissionsAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var result = new List<UserPermissionDto>();

                foreach (var user in users.Where(u => u.IsActive))
                {
                    var userPermissionResult = await GetUserPermissionsAsync(user.Id);
                    if (userPermissionResult.IsSuccess)
                    {
                        result.Add(userPermissionResult.Data);
                    }
                }

                return ServiceResult<List<UserPermissionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserPermissionDto>>.Failure($"Error al obtener permisos de usuarios: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateUserPermissionsAsync(UpdateUserPermissionsDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(updateDto.UserId);
                if (user == null)
                    return ServiceResult<bool>.Failure("Usuario no encontrado");

                foreach (var permissionUpdate in updateDto.Permissions)
                {
                    var existingUserPermission = await _permissionRepository.GetUserPermissionAsync(
                        updateDto.UserId, permissionUpdate.PermissionId);

                    if (existingUserPermission != null)
                    {
                        existingUserPermission.IsGranted = permissionUpdate.IsGranted;
                        await _permissionRepository.UpdateUserPermissionAsync(existingUserPermission);
                    }
                    else
                    {
                        // Crear nuevo permiso de usuario (ya sea para otorgar o denegar)
                        var newUserPermission = new UserPermission
                        {
                            UserId = updateDto.UserId,
                            PermissionId = permissionUpdate.PermissionId,
                            IsGranted = permissionUpdate.IsGranted
                        };
                        await _permissionRepository.AddUserPermissionAsync(newUserPermission);
                    }
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al actualizar permisos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> InitializeDefaultPermissionsAsync()
        {
            try
            {
                var existingPermissions = await _permissionRepository.GetAllAsync();

                // Solo inicializar si no hay permisos
                if (existingPermissions.Any())
                    return ServiceResult<bool>.Success(true);

                var defaultPermissions = DefaultPermissions.GetDefaultPermissions();
                var permissionsToCreate = new List<Permission>();

                foreach (var moduleDef in defaultPermissions.OrderBy(m => m.DisplayOrder))
                {
                    // Crear permiso de acceso al módulo
                    var modulePermission = new Permission
                    {
                        Name = $"{moduleDef.Module}.Access",
                        Description = $"Acceso al módulo {moduleDef.ModuleName}",
                        Module = moduleDef.Module,
                        Action = "Access",
                        Type = PermissionType.Module,
                        DisplayOrder = moduleDef.DisplayOrder * 100, // Espacio para ordenar
                        IsActive = true
                    };
                    permissionsToCreate.Add(modulePermission);

                    // Crear permisos de acciones
                    foreach (var actionDef in moduleDef.Actions.OrderBy(a => a.DisplayOrder))
                    {
                        var actionPermission = new Permission
                        {
                            Name = $"{moduleDef.Module}.{actionDef.Action}",
                            Description = actionDef.Name,
                            Module = moduleDef.Module,
                            Action = actionDef.Action,
                            Type = PermissionType.Action,
                            DisplayOrder = (moduleDef.DisplayOrder * 100) + actionDef.DisplayOrder,
                            IsActive = true
                        };
                        permissionsToCreate.Add(actionPermission);
                    }
                }

                // Guardar todos los permisos
                await _permissionRepository.CreateBulkAsync(permissionsToCreate);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al inicializar permisos: {ex.Message}");
            }
        }

        private string GetModuleDisplayName(string module)
        {
            return module switch
            {
                "Customer" => "Clientes",
                "Vehicle" => "Vehículos",
                "Rental" => "Alquileres",
                "Partner" => "Socios",
                "FinancialReports" => "Reportes Financieros",
                "User" => "Usuarios",
                "Audit" => "Auditoría",
                "Permission" => "Permisos",
                _ => module
            };
        }
    }
}