using CarRental.Core.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CarRental.Web.Services
{
    public class CompanySettingsApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public CompanySettingsApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<CompanySettings>> GetCompanySettingsAsync()
        {
            try
            {
                Console.WriteLine("[DEBUG] CompanySettingsApiClient - Iniciando petición GET");

                var response = await _httpClient.GetAsync("api/CompanySettings");

                Console.WriteLine($"[DEBUG] CompanySettingsApiClient - Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<CompanySettings>>();
                    Console.WriteLine($"[DEBUG] CompanySettingsApiClient - Result recibido: {result != null}");
                    return result ?? ApiResult<CompanySettings>.Failure("Error al deserializar respuesta");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - Error: {errorContent}");
                return ApiResult<CompanySettings>.Failure($"Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - Excepción: {ex.Message}");
                return ApiResult<CompanySettings>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResult<CompanySettings>> UpdateCompanySettingsAsync(CompanySettings settings)
        {
            try
            {
                Console.WriteLine("[DEBUG] CompanySettingsApiClient - Iniciando petición PUT");

                // ✅ Usar PutAsJsonAsync en lugar de PutAsync
                var response = await _httpClient.PutAsJsonAsync("api/CompanySettings", settings);

                Console.WriteLine($"[DEBUG] CompanySettingsApiClient - PUT Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<CompanySettings>>();
                    return result ?? ApiResult<CompanySettings>.Failure("Error al deserializar respuesta");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - PUT Error: {errorContent}");
                return ApiResult<CompanySettings>.Failure($"Error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - PUT Excepción: {ex.Message}");
                return ApiResult<CompanySettings>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> UploadLogoAsync(byte[] logoData)
        {
            try
            {
                Console.WriteLine($"[DEBUG] CompanySettingsApiClient - Subiendo logo ({logoData.Length} bytes)");

                using var content = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(logoData);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(fileContent, "logo", "logo.png");

                // ✅ PostAsync está disponible en AuthorizedHttpClient
                var response = await _httpClient.PostAsync("api/CompanySettings/upload-logo", content);

                Console.WriteLine($"[DEBUG] CompanySettingsApiClient - Upload Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true, "Logo actualizado exitosamente");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - Upload Error: {errorContent}");
                return ApiResult<bool>.Failure($"Error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CompanySettingsApiClient - Upload Excepción: {ex.Message}");
                return ApiResult<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }
}