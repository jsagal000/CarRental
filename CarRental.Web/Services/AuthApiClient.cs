using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net.Http.Json;

namespace CarRental.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<AuthResult>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<AuthResult>>();
                    return result ?? ApiResult<AuthResult>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<AuthResult>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return ApiResult<AuthResult>.Failure($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResult<AuthResult>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/auth/logout", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
                    return result ?? ApiResult<bool>.Success(true);
                }
                else
                {
                    return ApiResult<bool>.Failure($"Error HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error al cerrar sesión: {ex.Message}");
            }
        }

        public async Task<ApiResult<User>> ValidateTokenAsync(string token)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/validate-token", token);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                    return result ?? ApiResult<User>.Failure("Error al validar token");
                }
                else
                {
                    return ApiResult<User>.Failure($"Token inválido");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Failure($"Error al validar token: {ex.Message}");
            }
        }
    }
}
