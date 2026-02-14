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
    public class PartnerApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public PartnerApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<List<Partner>>> GetPartnersAsync()
        {
            try
            {
                Console.WriteLine("PartnerApiClient: Iniciando GetPartnersAsync");

                var response = await _httpClient.GetAsync("api/partners");
                Console.WriteLine($"PartnerApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var partners = await response.Content.ReadFromJsonAsync<List<Partner>>();
                    Console.WriteLine($"PartnerApiClient: Obtenidos {partners?.Count ?? 0} socios");
                    return ApiResult<List<Partner>>.Success(partners ?? new List<Partner>());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PartnerApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<List<Partner>>.Failure($"Error al obtener socios: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"PartnerApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<List<Partner>>.Failure($"Error al obtener socios: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"PartnerApiClient: Exception: {e.Message}");
                return ApiResult<List<Partner>>.Failure($"Error inesperado: {e.Message}");
            }
        }

        // Método alternativo que mantiene compatibilidad con el código existente
        public async Task<ApiResult<List<Partner>>> GetAllPartnersAsync()
        {
            return await GetPartnersAsync();
        }

        public async Task<ApiResult<Partner>> GetPartnerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/partners/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return ApiResult<Partner>.Failure($"Socio con ID {id} no encontrado.", (int)response.StatusCode);
                    }
                    return ApiResult<Partner>.Failure(errorContent, (int)response.StatusCode);
                }

                var partner = await response.Content.ReadFromJsonAsync<Partner>();
                return ApiResult<Partner>.Success(partner);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Partner>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Partner>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Partner>> AddPartnerAsync(PartnerForCreationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/partners", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Partner>.Failure(errorContent, (int)response.StatusCode);
                }

                var partner = await response.Content.ReadFromJsonAsync<Partner>();
                return ApiResult<Partner>.Success(partner);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Partner>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Partner>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdatePartnerAsync(Partner partner)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/partners/{partner.Id}", partner);
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

        public async Task<ApiResult<bool>> DeletePartnerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/partners/{id}");
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