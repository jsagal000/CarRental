using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net.Http.Json;

namespace CarRental.Web.Services
{
    public class PermissionApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;
        
        // ✅ CACHE ESTÁTICO: Compartido entre todas las instancias (persiste durante toda la sesión)
        private static readonly Dictionary<string, ModulePermissionsDto> _permissionsCache = new();
        private static readonly object _cacheLock = new object();

        public PermissionApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<bool>> HasPermissionAsync(string module, string action)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/permissions/check?module={module}&action={action}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
                    return result ?? ApiResult<bool>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    return ApiResult<bool>.Success(false);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error al verificar permisos: {ex.Message}");
            }
        }

        // ✅ OPTIMIZADO: Obtiene permisos del módulo (con cache en memoria)
        public async Task<ModulePermissionsDto> GetModulePermissionsAsync(string module)
        {
            // 1. Verificar si está en caché
            lock (_cacheLock)
            {
                if (_permissionsCache.TryGetValue(module, out var cachedPerms))
                {
                    Console.WriteLine($"✅ Cache HIT: Permisos de {module} obtenidos desde memoria");
                    return cachedPerms;
                }
            }

            // 2. Si no está en caché, llamar a la API
            try
            {
                Console.WriteLine($"🌐 Cache MISS: Solicitando permisos de {module} desde API");
                
                var response = await _httpClient.GetAsync($"api/permissions/module/{module}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<ModulePermissionsDto>>();
                    var perms = result?.Data ?? new ModulePermissionsDto();
                    
                    // 3. Guardar en caché
                    lock (_cacheLock)
                    {
                        _permissionsCache[module] = perms;
                    }
                    
                    return perms;
                }

                return new ModulePermissionsDto();
            }
            catch
            {
                return new ModulePermissionsDto();
            }
        }

        // ✅ MÉTODO ESTÁTICO: Limpiar el caché (útil al cerrar sesión)
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                _permissionsCache.Clear();
                Console.WriteLine("🗑️ Cache de permisos limpiado");
            }
        }

        public async Task<ApiResult<List<PermissionDto>>> GetAllPermissionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/permissions");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<List<PermissionDto>>>();
                    return result ?? ApiResult<List<PermissionDto>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<List<PermissionDto>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<List<PermissionDto>>.Failure($"Error al obtener permisos: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<PermissionModuleDto>>> GetPermissionsGroupedByModuleAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/permissions/grouped");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<List<PermissionModuleDto>>>();
                    return result ?? ApiResult<List<PermissionModuleDto>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<List<PermissionModuleDto>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<List<PermissionModuleDto>>.Failure($"Error al obtener permisos agrupados: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<UserPermissionDto>>> GetAllUsersPermissionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/permissions/users");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<List<UserPermissionDto>>>();
                    return result ?? ApiResult<List<UserPermissionDto>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<List<UserPermissionDto>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<List<UserPermissionDto>>.Failure($"Error al obtener permisos de usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResult<UserPermissionDto>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/permissions/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<UserPermissionDto>>();
                    return result ?? ApiResult<UserPermissionDto>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<UserPermissionDto>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<UserPermissionDto>.Failure($"Error al obtener permisos del usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdateUserPermissionsAsync(int userId, List<UserPermissionUpdate> permissions)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/permissions/user/{userId}", permissions);

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
                return ApiResult<bool>.Failure($"Error al actualizar permisos: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> InitializeDefaultPermissionsAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/permissions/initialize", null);

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
                return ApiResult<bool>.Failure($"Error al inicializar permisos: {ex.Message}");
            }
        }
    }
}