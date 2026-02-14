// CarRental.Web/Services/VehicleApiClient.cs
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CarRental.Web.Services
{
    public class VehicleApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public VehicleApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<List<Vehicle>>> GetVehiclesAsync()
        {
            try
            {
                Console.WriteLine("VehicleApiClient: Iniciando GetVehiclesAsync");

                var response = await _httpClient.GetAsync("api/vehicles");
                Console.WriteLine($"VehicleApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var vehicles = await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                    Console.WriteLine($"VehicleApiClient: Obtenidos {vehicles?.Count ?? 0} vehículos");
                    return ApiResult<List<Vehicle>>.Success(vehicles ?? new List<Vehicle>());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"VehicleApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<List<Vehicle>>.Failure($"Error al obtener vehículos: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"VehicleApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<List<Vehicle>>.Failure($"Error al obtener vehículos: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"VehicleApiClient: Exception: {e.Message}");
                return ApiResult<List<Vehicle>>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Vehicle>> GetVehicleByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/vehicles/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return ApiResult<Vehicle>.Failure($"Vehículo con ID {id} no encontrado.", (int)response.StatusCode);
                    }
                    return ApiResult<Vehicle>.Failure(errorContent, (int)response.StatusCode);
                }

                var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();
                return ApiResult<Vehicle>.Success(vehicle);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Vehicle>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Vehicle>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Vehicle>> AddVehicleAsync(VehicleForCreationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/vehicles", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Vehicle>.Failure(errorContent, (int)response.StatusCode);
                }

                var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();
                return ApiResult<Vehicle>.Success(vehicle);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Vehicle>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Vehicle>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdateVehicleAsync(VehicleForUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/vehicles/{dto.Id}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<bool>.Failure(errorContent, (int)response.StatusCode);
                }

                return ApiResult<bool>.Success(true);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<bool>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<bool>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<bool>> DeleteVehicleAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/vehicles/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<bool>.Failure(errorContent, (int)response.StatusCode);
                }

                return ApiResult<bool>.Success(true);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<bool>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<bool>.Failure($"Error inesperado: {e.Message}");
            }
        }
    }
}