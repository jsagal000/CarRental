using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace CarRental.Web.Services
{
    public class AuthorizedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateService _authState;
        private readonly IJSRuntime _jsRuntime;

        public AuthorizedHttpClient(HttpClient httpClient, AuthStateService authState, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authState = authState;
            _jsRuntime = jsRuntime;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.PostAsJsonAsync(requestUri, value);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.PutAsJsonAsync(requestUri, value);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.DeleteAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.PostAsync(requestUri, content);
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            try
            {
                // Intentar obtener el token del AuthState primero
                string token = _authState.Token;

                // Si no está disponible en AuthState, intentar obtenerlo de localStorage
                if (string.IsNullOrEmpty(token))
                {
                    token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    Console.WriteLine($"AuthorizedHttpClient: Token configurado - {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    Console.WriteLine("AuthorizedHttpClient: Warning - No se encontró token de autenticación");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuthorizedHttpClient: Error al configurar header de autorización: {ex.Message}");
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}