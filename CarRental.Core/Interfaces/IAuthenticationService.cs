using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;

namespace CarRental.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResult> LoginAsync(LoginDto loginDto);
        Task<ServiceResult<bool>> LogoutAsync(string token);
        string GenerateJwtToken(User user);
        Task<ServiceResult<User>> ValidateTokenAsync(string token);
        bool IsTokenExpired(string token);
    }
}