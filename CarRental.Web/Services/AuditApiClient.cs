using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net.Http.Json;
using System.Text;

namespace CarRental.Web.Services
{
    public class AuditApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public AuditApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            try
            {
                var queryParams = BuildQueryString(filter);
                Console.WriteLine($"[API CLIENT] Query: api/audit?{queryParams}"); // Debug

                var response = await _httpClient.GetAsync($"api/audit?{queryParams}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<PagedResult<AuditLogDto>>>();
                    return result ?? ApiResult<PagedResult<AuditLogDto>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API CLIENT ERROR] {errorContent}"); // Debug
                    return ApiResult<PagedResult<AuditLogDto>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return ApiResult<PagedResult<AuditLogDto>>.Failure($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResult<PagedResult<AuditLogDto>>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<AuditLogDto>>> GetUserActivityAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/audit/user/{userId}{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<List<AuditLogDto>>>();
                    return result ?? ApiResult<List<AuditLogDto>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<List<AuditLogDto>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<List<AuditLogDto>>.Failure($"Error al obtener actividad del usuario: {ex.Message}");
            }
        }

        public async Task<ApiResult<Dictionary<string, int>>> GetActivitySummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/audit/summary{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<Dictionary<string, int>>>();
                    return result ?? ApiResult<Dictionary<string, int>>.Failure("Error al deserializar respuesta");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Dictionary<string, int>>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<Dictionary<string, int>>.Failure($"Error al obtener resumen: {ex.Message}");
            }
        }

        private string BuildQueryString(AuditLogFilterDto filter)
        {
            var queryParams = new List<string>();

            // Solo agregar parámetros que tienen valor
            if (filter.StartDate.HasValue)
                queryParams.Add($"StartDate={filter.StartDate.Value:yyyy-MM-dd}");

            if (filter.EndDate.HasValue)
                queryParams.Add($"EndDate={filter.EndDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrWhiteSpace(filter.Module))
                queryParams.Add($"Module={Uri.EscapeDataString(filter.Module)}");

            if (!string.IsNullOrWhiteSpace(filter.Action))
                queryParams.Add($"Action={Uri.EscapeDataString(filter.Action)}");

            if (filter.UserId.HasValue && filter.UserId.Value > 0)
                queryParams.Add($"UserId={filter.UserId.Value}");

            if (!string.IsNullOrWhiteSpace(filter.UserName))
                queryParams.Add($"UserName={Uri.EscapeDataString(filter.UserName)}");

            if (filter.IsSuccess.HasValue)
                queryParams.Add($"IsSuccess={filter.IsSuccess.Value}");

            // Siempre agregar paginación
            queryParams.Add($"Page={filter.Page}");
            queryParams.Add($"PageSize={filter.PageSize}");

            return string.Join("&", queryParams);
        }
    }
}