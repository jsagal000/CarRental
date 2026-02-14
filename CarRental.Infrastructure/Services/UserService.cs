using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Interfaces;

namespace CarRental.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResult<IEnumerable<User>>> GetUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return ServiceResult<IEnumerable<User>>.Success(users);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<User>>.Failure($"Error al obtener usuarios: {ex.Message}");
            }
        }

        public async Task<ServiceResult<User>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return ServiceResult<User>.Failure("Usuario no encontrado");

                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<User>> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                    return ServiceResult<User>.Failure("Usuario no encontrado");

                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<User>> CreateUserAsync(UserForCreationDto userDto)
        {
            try
            {
                // Validar que no exista el username
                if (await _userRepository.UsernameExistsAsync(userDto.Username))
                    return ServiceResult<User>.Failure("El nombre de usuario ya existe");

                // Validar que no exista el email
                if (await _userRepository.EmailExistsAsync(userDto.Email))
                    return ServiceResult<User>.Failure("El email ya está registrado");

                var user = new User
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = _passwordHasher.HashPassword(userDto.Password),
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Role = userDto.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.AddAsync(user);
                return ServiceResult<User>.Success(createdUser);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<User>> UpdateUserAsync(int id, UserForUpdateDto userDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return ServiceResult<User>.Failure("Usuario no encontrado");

                // Validar que no exista el email en otro usuario
                var existingEmailUser = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingEmailUser != null && existingEmailUser.Id != id)
                    return ServiceResult<User>.Failure("El email ya está registrado");

                user.Email = userDto.Email;
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Role = userDto.Role;
                user.IsActive = userDto.IsActive;

                await _userRepository.UpdateAsync(user);
                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return ServiceResult<bool>.Failure("Usuario no encontrado");

                await _userRepository.DeleteAsync(id);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Failure("Usuario no encontrado");

                // Verificar contraseña actual
                if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                    return ServiceResult<bool>.Failure("La contraseña actual es incorrecta");

                // Actualizar contraseña
                user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
                await _userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al cambiar contraseña: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                    return ServiceResult<bool>.Failure("Credenciales inválidas");

                var isValidPassword = _passwordHasher.VerifyPassword(password, user.PasswordHash);
                if (!isValidPassword)
                    return ServiceResult<bool>.Failure("Credenciales inválidas");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al validar credenciales: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateLastLoginAsync(int userId)
        {
            try
            {
                await _userRepository.UpdateLastLoginAsync(userId, DateTime.UtcNow);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al actualizar último login: {ex.Message}");
            }
        }
    }
}