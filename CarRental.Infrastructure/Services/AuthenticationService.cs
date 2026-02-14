using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarRental.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionRepository _sessionRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            IUserSessionRepository sessionRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
                if (user == null || !user.IsActive)
                    return AuthResult.Failure("Credenciales inválidas");

                var isValidPassword = _passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash);
                if (!isValidPassword)
                    return AuthResult.Failure("Credenciales inválidas");

                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(8); // 8 horas de duración

                // Guardar sesión
                var session = new UserSession
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = expiresAt
                };
                await _sessionRepository.AddAsync(session);

                // Actualizar último login
                await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

                return AuthResult.Success(token, user, expiresAt);
            }
            catch (Exception ex)
            {
                return AuthResult.Failure($"Error en autenticación: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> LogoutAsync(string token)
        {
            try
            {
                await _sessionRepository.RevokeSessionAsync(token);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al cerrar sesión: {ex.Message}");
            }
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("FullName", user.FullName)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResult<User>> ValidateTokenAsync(string token)
        {
            try
            {
                var session = await _sessionRepository.GetByTokenAsync(token);
                if (session == null)
                    return ServiceResult<User>.Failure("Token inválido");

                return ServiceResult<User>.Success(session.User);
            }
            catch (Exception ex)
            {
                return ServiceResult<User>.Failure($"Error al validar token: {ex.Message}");
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                return jwt.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}