using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;

namespace CarRental.Core.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<IEnumerable<User>>> GetUsersAsync();
        Task<ServiceResult<User>> GetUserByIdAsync(int id);
        Task<ServiceResult<User>> GetUserByUsernameAsync(string username);
        Task<ServiceResult<User>> CreateUserAsync(UserForCreationDto userDto);
        Task<ServiceResult<User>> UpdateUserAsync(int id, UserForUpdateDto userDto);
        Task<ServiceResult<bool>> DeleteUserAsync(int id);
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ServiceResult<bool>> ValidateUserCredentialsAsync(string username, string password);
        Task<ServiceResult<bool>> UpdateLastLoginAsync(int userId);
    }
}