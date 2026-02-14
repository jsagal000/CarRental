// CarRental.Web/Services/CustomerApiClient.cs
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
    public class CustomerApiClient
    {
        private readonly AuthorizedHttpClient _httpClient; // Cambio: usar AuthorizedHttpClient

        public CustomerApiClient(AuthorizedHttpClient httpClient) // Cambio: inyectar AuthorizedHttpClient
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<List<Customer>>> GetAllCustomersAsync()
        {
            try
            {
                Console.WriteLine("CustomerApiClient: Iniciando GetAllCustomersAsync");

                var response = await _httpClient.GetAsync("api/customers");
                Console.WriteLine($"CustomerApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
                    Console.WriteLine($"CustomerApiClient: Obtenidos {customers?.Count ?? 0} clientes");
                    return ApiResult<List<Customer>>.Success(customers ?? new List<Customer>());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"CustomerApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<List<Customer>>.Failure($"Error al obtener clientes: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"CustomerApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<List<Customer>>.Failure($"Error al obtener clientes: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"CustomerApiClient: Exception: {e.Message}");
                return ApiResult<List<Customer>>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Customer>> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/customers/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return ApiResult<Customer>.Failure($"Cliente con ID {id} no encontrado.", (int)response.StatusCode);
                    }
                    return ApiResult<Customer>.Failure(errorContent, (int)response.StatusCode);
                }

                var customer = await response.Content.ReadFromJsonAsync<Customer>();
                return ApiResult<Customer>.Success(customer);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Customer>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Customer>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Customer>> AddCustomerAsync(CustomerForCreationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/customers", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Customer>.Failure(errorContent, (int)response.StatusCode);
                }

                var customer = await response.Content.ReadFromJsonAsync<Customer>();
                return ApiResult<Customer>.Success(customer);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Customer>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Customer>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/customers/{customer.Id}", customer);
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

        public async Task<ApiResult<bool>> DeleteCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/customers/{id}");
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