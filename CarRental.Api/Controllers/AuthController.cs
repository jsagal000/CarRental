using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResult<AuthResult>>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<AuthResult>.Failure($"Datos de login inválidos: {errors}"));
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResult<AuthResult>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<AuthResult>.Success(result));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResult<bool>>> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResult<bool>.Failure("Token requerido"));
            }

            var result = await _authService.LogoutAsync(token);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResult<bool>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<bool>.Success(true));
        }

        [HttpPost("validate-token")]
        public async Task<ActionResult<ApiResult<User>>> ValidateToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResult<User>.Failure("Token requerido"));
            }

            var result = await _authService.ValidateTokenAsync(token);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResult<User>.Failure(result.ErrorMessage));
            }

            return Ok(ApiResult<User>.Success(result.Data));
        }


    }


}
