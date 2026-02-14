using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Web.Services;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Net.Http.Json;

namespace CarRental.Web.Services
{
    public class AuthStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthApiClient _authApiClient;
        private readonly HttpClient _httpClient; // Usar HttpClient directamente

        // Cache de permisos
        private Dictionary<string, bool> _permissions = new();
        private bool _permissionsLoaded = false;

        public User CurrentUser { get; private set; }
        public string Token { get; private set; }
        public bool IsAuthenticated => CurrentUser != null && !string.IsNullOrEmpty(Token);

        public event Action OnAuthenticationStateChanged;

        public AuthStateService(IJSRuntime jsRuntime, AuthApiClient authApiClient, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _authApiClient = authApiClient;
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userJson))
                {
                    Token = token;
                    CurrentUser = JsonSerializer.Deserialize<User>(userJson);

                    // Cargar permisos automáticamente si está autenticado
                    await LoadUserPermissionsAsync();

                    NotifyAuthenticationStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando AuthState: {ex.Message}");
                await LogoutAsync();
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var result = await _authApiClient.LoginAsync(new LoginDto
                {
                    Username = username,
                    Password = password
                });

                if (result.IsSuccess)
                {
                    CurrentUser = result.Data.User;
                    Token = result.Data.Token;

                    // Guardar en localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", Token);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", JsonSerializer.Serialize(CurrentUser));

                    // Cargar permisos inmediatamente después del login
                    await LoadUserPermissionsAsync();

                    NotifyAuthenticationStateChanged();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            Token = null;

            // Limpiar permisos
            _permissions.Clear();
            _permissionsLoaded = false;

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");

            NotifyAuthenticationStateChanged();
        }

        // Cargar permisos usando HttpClient directamente
        public async Task LoadUserPermissionsAsync()
        {
            if (_permissionsLoaded || CurrentUser == null)
                return;

            try
            {
                // Configurar header de autorización
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                var response = await _httpClient.GetAsync($"api/permissions/user/{CurrentUser.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<UserPermissionDto>>();
                    if (result?.IsSuccess == true)
                    {
                        _permissions = result.Data.Permissions
                            .ToDictionary(p => $"{p.Module}.{p.Action}", p => p.IsGranted);
                        _permissionsLoaded = true;

                        Console.WriteLine($"Permisos cargados: {_permissions.Count} permisos para {CurrentUser.Username}");
                    }
                    else
                    {
                        LoadPermissionsByRole();
                    }
                }
                else
                {
                    LoadPermissionsByRole();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando permisos: {ex.Message}");
                LoadPermissionsByRole();
            }
        }

        // Verificación instantánea (sin async)
        public bool HasPermission(string module, string action)
        {
            if (!IsAuthenticated)
                return false;

            string key = $"{module}.{action}";

            // Si los permisos están cargados, usar el cache
            if (_permissionsLoaded)
            {
                return _permissions.GetValueOrDefault(key, false);
            }

            // Fallback por rol mientras cargan los permisos específicos
            return HasPermissionByRole(module, action);
        }

        // Resto del código igual...
        private bool HasPermissionByRole(string module, string action)
        {
            if (CurrentUser == null)
                return false;

            return CurrentUser.Role switch
            {
                UserRole.Admin => true,
                UserRole.Manager => !(module == "User" && action != "View") && module != "Permission",
                UserRole.Employee => action switch
                {
                    "View" => true,
                    "Create" => module != "User",
                    "Edit" => module is "Customer" or "Vehicle" or "Rental",
                    "Delete" => false,
                    _ => false
                },
                UserRole.ReadOnly => action == "View",
                _ => false
            };
        }

        private void LoadPermissionsByRole()
        {
            if (CurrentUser == null) return;

            _permissions.Clear();
            var modules = new[] { "Customer", "Vehicle", "Rental", "Partner", "User", "Audit", "Permission" };
            var actions = new[] { "View", "Create", "Edit", "Delete", "Manage" };

            foreach (var module in modules)
            {
                foreach (var action in actions)
                {
                    if ((module != "Permission" && action == "Manage") ||
                        (module != "Audit" && action == "Manage"))
                        continue;

                    string key = $"{module}.{action}";
                    _permissions[key] = HasPermissionByRole(module, action);
                }
            }

            _permissionsLoaded = true;
        }

        // Propiedades de conveniencia
        public bool CanManageVehicles => HasPermission("Vehicle", "View");
        public bool CanViewFinancialReports => CurrentUser?.Role is UserRole.Admin or UserRole.Manager;
        public bool IsManagerOrAbove => CurrentUser?.Role is UserRole.Admin or UserRole.Manager;
        public bool CanManageUsers => CurrentUser?.Role == UserRole.Admin;

        private void NotifyAuthenticationStateChanged()
        {
            OnAuthenticationStateChanged?.Invoke();
        }
    }
}