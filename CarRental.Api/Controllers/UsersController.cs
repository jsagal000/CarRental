using CarRental.Api.Extensions;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;

        public UsersController(IUserService userService, IAuditService auditService)
        {
            _userService = userService;
            _auditService = auditService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<IEnumerable<User>>>> GetUsers()
        {
            var result = await _userService.GetUsersAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<IEnumerable<User>>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<IEnumerable<User>>.Success(result.Data));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<User>>> GetUser(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResult<User>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<User>.Success(result.Data));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<User>>> CreateUser([FromBody] UserForCreationDto userDto)
        {
            var currentUserId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<User>.Failure($"Datos inválidos: {errors}"));
            }

            var result = await _userService.CreateUserAsync(userDto);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<User>.Failure(result.ErrorMessage));
            }

            // ✅ AUDITAR: Creación de usuario
            await _auditService.LogActionAsync(
                userId: currentUserId,
                module: "User",
                action: "Create",
                entityId: result.Data.Id,
                entityName: $"{result.Data.Username}",
                description: $"Creó nuevo usuario: {result.Data.Username} ({result.Data.Role})",
                newValues: new
                {
                    result.Data.Id,
                    result.Data.Username,
                    result.Data.Email,
                    result.Data.FirstName,
                    result.Data.LastName,
                    result.Data.Role,
                    result.Data.IsActive
                },
                ipAddress: HttpContext.GetClientIpAddress(),
                userAgent: HttpContext.GetUserAgent()
            );

            return CreatedAtAction(nameof(GetUser),
                new { id = result.Data.Id },
                ApiResult<User>.Success(result.Data));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<User>>> UpdateUser(int id, [FromBody] UserForUpdateDto userDto)
        {
            var currentUserId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<User>.Failure($"Datos inválidos: {errors}"));
            }

            // Obtener datos anteriores para auditoría
            var oldUserResult = await _userService.GetUserByIdAsync(id);
            var oldUserData = oldUserResult.IsSuccess ? new
            {
                oldUserResult.Data.Username,
                oldUserResult.Data.Email,
                oldUserResult.Data.FirstName,
                oldUserResult.Data.LastName,
                oldUserResult.Data.Role,
                oldUserResult.Data.IsActive
            } : null;

            var result = await _userService.UpdateUserAsync(id, userDto);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<User>.Failure(result.ErrorMessage));
            }

            // ✅ AUDITAR: Edición de usuario
            await _auditService.LogActionAsync(
                userId: currentUserId,
                module: "User",
                action: "Edit",
                entityId: result.Data.Id,
                entityName: $"{result.Data.Username}",
                description: $"Editó usuario: {result.Data.Username}",
                oldValues: oldUserData,
                newValues: new
                {
                    result.Data.Id,
                    result.Data.Username,
                    result.Data.Email,
                    result.Data.FirstName,
                    result.Data.LastName,
                    result.Data.Role,
                    result.Data.IsActive
                },
                ipAddress: HttpContext.GetClientIpAddress(),
                userAgent: HttpContext.GetUserAgent()
            );

            return Ok(ApiResult<User>.Success(result.Data));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<bool>>> DeleteUser(int id)
        {
            var currentUserId = GetCurrentUserId();

            // Verificar que no se esté eliminando a sí mismo
            if (currentUserId == id)
            {
                return BadRequest(ApiResult<bool>.Failure("No puedes eliminarte a ti mismo"));
            }

            // Obtener datos del usuario antes de eliminar
            var userToDeleteResult = await _userService.GetUserByIdAsync(id);
            var userToDelete = userToDeleteResult.IsSuccess ? userToDeleteResult.Data : null;

            var result = await _userService.DeleteUserAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            // ✅ AUDITAR: Eliminación de usuario
            if (userToDelete != null)
            {
                await _auditService.LogActionAsync(
                    userId: currentUserId,
                    module: "User",
                    action: "Delete",
                    entityId: id,
                    entityName: $"{userToDelete.Username}",
                    description: $"Eliminó usuario: {userToDelete.Username} ({userToDelete.Role})",
                    oldValues: new
                    {
                        userToDelete.Id,
                        userToDelete.Username,
                        userToDelete.Email,
                        userToDelete.FirstName,
                        userToDelete.LastName,
                        userToDelete.Role,
                        userToDelete.IsActive
                    },
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
            }

            return Ok(ApiResult<bool>.Success(true));
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResult<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var currentUserId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<bool>.Failure($"Datos inválidos: {errors}"));
            }

            var result = await _userService.ChangePasswordAsync(currentUserId, changePasswordDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogActionAsync(
                    userId: currentUserId,
                    module: "User",
                    action: "PasswordChange",
                    description: "Intento fallido de cambio de contraseña",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: result.ErrorMessage
                );

                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            // ✅ AUDITAR: Cambio de contraseña exitoso
            await _auditService.LogActionAsync(
                userId: currentUserId,
                module: "User",
                action: "PasswordChange",
                description: "Cambió su contraseña exitosamente",
                ipAddress: HttpContext.GetClientIpAddress(),
                userAgent: HttpContext.GetUserAgent()
            );

            return Ok(ApiResult<bool>.Success(true));
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResult<User>>> GetProfile()
        {
            var currentUserId = GetCurrentUserId();
            var result = await _userService.GetUserByIdAsync(currentUserId);

            if (!result.IsSuccess)
            {
                return NotFound(ApiResult<User>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<User>.Success(result.Data));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}