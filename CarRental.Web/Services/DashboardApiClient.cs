// CarRental.Web/Services/DashboardApiClient.cs
using CarRental.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CarRental.Web.Services
{
    public class DashboardApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public DashboardApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<DashboardDataDto>> GetDashboardDataAsync()
        {
            try
            {
                Console.WriteLine("DashboardApiClient: Iniciando GetDashboardDataAsync");

                var response = await _httpClient.GetAsync("api/dashboard");
                Console.WriteLine($"DashboardApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<DashboardDataDto>();
                    Console.WriteLine($"DashboardApiClient: Obtenidos {data?.ActiveRentals?.Count ?? 0} alquileres activos y {data?.TodayBirthdays?.Count ?? 0} cumpleaños");
                    return ApiResult<DashboardDataDto>.Success(data);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DashboardApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<DashboardDataDto>.Failure($"Error al obtener datos del dashboard: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"DashboardApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<DashboardDataDto>.Failure($"Error al obtener datos del dashboard: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"DashboardApiClient: Exception: {e.Message}");
                return ApiResult<DashboardDataDto>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<DashboardStatisticsDto>> GetDashboardStatisticsAsync()
        {
            try
            {
                Console.WriteLine("DashboardApiClient: Iniciando GetDashboardStatisticsAsync");

                var response = await _httpClient.GetAsync("api/dashboard/statistics");
                Console.WriteLine($"DashboardApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var statistics = await response.Content.ReadFromJsonAsync<DashboardStatisticsDto>();
                    Console.WriteLine($"DashboardApiClient: Estadísticas obtenidas exitosamente");
                    return ApiResult<DashboardStatisticsDto>.Success(statistics);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DashboardApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<DashboardStatisticsDto>.Failure($"Error al obtener estadísticas: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"DashboardApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<DashboardStatisticsDto>.Failure($"Error al obtener estadísticas: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"DashboardApiClient: Exception: {e.Message}");
                return ApiResult<DashboardStatisticsDto>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<List<VehicleInfoDto>>> GetVehiclesInfoAsync()
        {
            try
            {
                Console.WriteLine("DashboardApiClient: Iniciando GetVehiclesInfoAsync");

                var response = await _httpClient.GetAsync("api/dashboard/vehicles");
                Console.WriteLine($"DashboardApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleInfoDto>>();
                    Console.WriteLine($"DashboardApiClient: Obtenidos {vehicles?.Count ?? 0} vehículos");
                    return ApiResult<List<VehicleInfoDto>>.Success(vehicles);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DashboardApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<List<VehicleInfoDto>>.Failure($"Error al obtener vehículos: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"DashboardApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<List<VehicleInfoDto>>.Failure($"Error al obtener vehículos: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"DashboardApiClient: Exception: {e.Message}");
                return ApiResult<List<VehicleInfoDto>>.Failure($"Error inesperado: {e.Message}");
            }
        }
    }

    // DTOs para el Dashboard
    public class DashboardDataDto
    {
        public List<ActiveRentalDto> ActiveRentals { get; set; } = new();
        public List<CustomerBirthdayDto> TodayBirthdays { get; set; } = new();
    }

    public class ActiveRentalDto
    {
        public int RentalId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string VehicleName { get; set; }
        public string VehiclePlate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalCost { get; set; }
        public string DestinationType { get; set; }
        public string DestinationCity { get; set; }
    }

    public class CustomerBirthdayDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public int BirthYear { get; set; }
    }

    public class VehicleInfoDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal DailyRate { get; set; }
        public string State { get; set; }
        public string Ownership { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class DashboardStatisticsDto
    {
        public int TotalActiveRentals { get; set; }
        public int TotalReservedRentals { get; set; }
        public int TotalOverdueRentals { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalCustomers { get; set; }
    }
}