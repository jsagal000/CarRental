using CarRental.Api.Attributes;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IAuditService _auditService;

        public PermissionsController(IPermissionService permissionService, IAuditService auditService)
        {
            _permissionService = permissionService;
            _auditService = auditService;
        }

        [HttpGet]
        [RequirePermission("Permission", "View")]
        public async Task<ActionResult<ApiResult<List<PermissionDto>>>> GetAllPermissions()
        {
            var result = await _permissionService.GetAllPermissionsAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<List<PermissionDto>>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<List<PermissionDto>>.Success(result.Data));
        }

        [HttpGet("grouped")]
        [RequirePermission("Permission", "View")]
        public async Task<ActionResult<ApiResult<List<PermissionModuleDto>>>> GetPermissionsGroupedByModule()
        {
            var result = await _permissionService.GetPermissionsGroupedByModuleAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<List<PermissionModuleDto>>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<List<PermissionModuleDto>>.Success(result.Data));
        }

        [HttpGet("users")]
        [RequirePermission("Permission", "Manage")]
        public async Task<ActionResult<ApiResult<List<UserPermissionDto>>>> GetAllUsersPermissions()
        {
            var result = await _permissionService.GetAllUsersPermissionsAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<List<UserPermissionDto>>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<List<UserPermissionDto>>.Success(result.Data));
        }

        [HttpGet("user/{userId}")]
        [RequirePermission("Permission", "Manage")]
        public async Task<ActionResult<ApiResult<UserPermissionDto>>> GetUserPermissions(int userId)
        {
            var result = await _permissionService.GetUserPermissionsAsync(userId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<UserPermissionDto>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<UserPermissionDto>.Success(result.Data));
        }

        [HttpPut("user/{userId}")]
        [RequirePermission("Permission", "Manage")]
        public async Task<ActionResult<ApiResult<bool>>> UpdateUserPermissions(int userId, [FromBody] List<UserPermissionUpdate> permissions)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var updateDto = new UpdateUserPermissionsDto
            {
                UserId = userId,
                Permissions = permissions
            };

            var result = await _permissionService.UpdateUserPermissionsAsync(updateDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogActionAsync(
                    currentUserId,
                    "Permission",
                    "Manage",
                    userId,
                    "UserPermissions",
                    $"Error al actualizar permisos del usuario {userId}",
                    null,
                    null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString(),
                    false,
                    result.ErrorMessage
                );

                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            await _auditService.LogActionAsync(
                currentUserId,
                "Permission",
                "Manage",
                userId,
                "UserPermissions",
                $"Permisos actualizados para el usuario {userId}. Total de cambios: {permissions.Count}",
                null,
                permissions,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString()
            );

            return Ok(ApiResult<bool>.Success(true));
        }

        [HttpGet("check")]
        public async Task<ActionResult<ApiResult<bool>>> CheckPermission([FromQuery] string module, [FromQuery] string action)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (currentUserId == 0)
            {
                return BadRequest(ApiResult<bool>.Failure("Usuario no autenticado"));
            }

            var result = await _permissionService.HasPermissionAsync(currentUserId, module, action);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<bool>.Success(result.Data));
        }

        [HttpPost("initialize")]
        [RequirePermission("Permission", "Manage")]
        public async Task<ActionResult<ApiResult<bool>>> InitializeDefaultPermissions()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _permissionService.InitializeDefaultPermissionsAsync();

            if (!result.IsSuccess)
            {
                await _auditService.LogActionAsync(
                    currentUserId,
                    "Permission",
                    "Manage",
                    null,
                    "DefaultPermissions",
                    "Error al inicializar permisos por defecto",
                    null,
                    null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString(),
                    false,
                    result.ErrorMessage
                );

                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            await _auditService.LogActionAsync(
                currentUserId,
                "Permission",
                "Manage",
                null,
                "DefaultPermissions",
                "Permisos por defecto inicializados correctamente",
                null,
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString()
            );

            return Ok(ApiResult<bool>.Success(true));
        }
    }
}