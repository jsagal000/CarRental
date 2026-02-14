using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net.Http.Json;

namespace CarRental.Web.Services
{
    public class UserApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public UserApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<IEnumerable<User>>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<IEnumerable<User>>>();
                    return result ?? ApiResult<IEnumerable<User>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<IEnumerable<User>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return ApiResult<IEnumerable<User>>.Failure($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResult<IEnumerable<User>>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResult<User>> GetUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                    return result ?? ApiResult<User>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    return ApiResult<User>.Failure($"Error HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Failure($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<User>> CreateUserAsync(UserForCreationDto userDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users", userDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                    return result ?? ApiResult<User>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<User>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Failure($"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<User>> UpdateUserAsync(int id, UserForUpdateDto userDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", userDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                    return result ?? ApiResult<User>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<User>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Failure($"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
                    return result ?? ApiResult<bool>.Success(true);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<bool>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/change-password", changePasswordDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
                    return result ?? ApiResult<bool>.Success(true);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<bool>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error al cambiar contraseña: {ex.Message}");
            }
        }

        public async Task<ApiResult<User>> GetProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users/profile");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                    return result ?? ApiResult<User>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    return ApiResult<User>.Failure($"Error HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Failure($"Error al obtener perfil: {ex.Message}");
            }
        }
    }
}