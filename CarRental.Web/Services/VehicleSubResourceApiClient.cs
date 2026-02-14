using CarRental.Core.Models;
using CarRental.Web.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Text;

namespace CarRental.Web.Services
{
    public class VehicleSubResourceApiClient<TEntity, TDto> : IVehicleSubResourceApiClient<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _resourceName;

        public VehicleSubResourceApiClient(HttpClient httpClient, string resourceName)
        {
            _httpClient = httpClient;
            _resourceName = resourceName;
        }

        private async Task<ApiResponse> HandleRequest(Func<Task<HttpResponseMessage>> requestFunc)
        {
            try
            {
                var response = await requestFunc();
                if (response.IsSuccessStatusCode) return new ApiResponse { IsSuccess = true };

                var errorResponse = new ApiResponse { IsSuccess = false };
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                    errorResponse.ErrorMessage = "Por favor, corrija los errores de validación.";
                    errorResponse.ValidationErrors = validationResponse?.Errors;
                }
                else
                {
                    errorResponse.ErrorMessage = $"Error desde la API: {response.ReasonPhrase}";
                }
                return errorResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error de conexión: {ex.Message}" };
            }
        }

        // Métodos existentes
        public async Task<ApiResponse<List<TEntity>>> GetByVehicleIdAsync(int vehicleId)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<TEntity>>($"api/vehicles/{vehicleId}/{_resourceName}");
                return new ApiResponse<List<TEntity>> { IsSuccess = true, Data = data ?? new List<TEntity>() };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TEntity>> { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse> CreateAsync(int vehicleId, TDto dto)
        {
            return await HandleRequest(() => _httpClient.PostAsJsonAsync($"api/vehicles/{vehicleId}/{_resourceName}", dto));
        }

        public async Task<ApiResponse> UpdateAsync(int entityId, TEntity entity)
        {
            return await HandleRequest(() => _httpClient.PutAsJsonAsync($"api/{_resourceName}/{entityId}", entity));
        }

        public async Task<ApiResponse> DeleteAsync(int entityId)
        {
            return await HandleRequest(() => _httpClient.DeleteAsync($"api/{_resourceName}/{entityId}"));
        }

        // Nuevos métodos para reportes
        public async Task<ApiResponse<List<TEntity>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int vehicleId = 0)
        {
            try
            {
                var queryParams = new StringBuilder();
                queryParams.Append($"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

                if (vehicleId > 0)
                {
                    queryParams.Append($"&vehicleId={vehicleId}");
                }

                var url = $"api/{_resourceName}/by-date-range{queryParams}";
                var data = await _httpClient.GetFromJsonAsync<List<TEntity>>(url);

                return new ApiResponse<List<TEntity>>
                {
                    IsSuccess = true,
                    Data = data ?? new List<TEntity>()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TEntity>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error obteniendo datos por rango de fechas: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<TEntity>>> GetAllAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<TEntity>>($"api/{_resourceName}");
                return new ApiResponse<List<TEntity>>
                {
                    IsSuccess = true,
                    Data = data ?? new List<TEntity>()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TEntity>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error obteniendo todos los datos: {ex.Message}"
                };
            }
        }
    }
}