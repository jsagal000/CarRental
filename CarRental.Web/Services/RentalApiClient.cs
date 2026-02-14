using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.JSInterop;

namespace CarRental.Web.Services
{
    public class RentalApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public RentalApiClient(AuthorizedHttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResult<List<Rental>>> GetRentalsAsync()
        {
            try
            {
                Console.WriteLine("RentalApiClient: Iniciando GetRentalsAsync");

                var response = await _httpClient.GetAsync("api/rentals");
                Console.WriteLine($"RentalApiClient: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rentals = await response.Content.ReadFromJsonAsync<List<Rental>>();
                    Console.WriteLine($"RentalApiClient: Obtenidos {rentals?.Count ?? 0} alquileres");
                    return ApiResult<List<Rental>>.Success(rentals ?? new List<Rental>());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"RentalApiClient: Error {response.StatusCode}: {errorContent}");
                    return ApiResult<List<Rental>>.Failure($"Error al obtener alquileres: {response.StatusCode} - {errorContent}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"RentalApiClient: HttpRequestException: {e.Message}");
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<List<Rental>>.Failure($"Error al obtener alquileres: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RentalApiClient: Exception: {e.Message}");
                return ApiResult<List<Rental>>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Rental>> GetRentalByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/rentals/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return ApiResult<Rental>.Failure($"Alquiler con ID {id} no encontrado.", (int)response.StatusCode);
                    }
                    return ApiResult<Rental>.Failure(errorContent, (int)response.StatusCode);
                }
                var rental = await response.Content.ReadFromJsonAsync<Rental>();
                return ApiResult<Rental>.Success(rental);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Rental>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Rental>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<Rental>> AddRentalAsync(RentalForCreationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/rentals", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Rental>.Failure(errorContent, (int)response.StatusCode);
                }
                var createdRental = await response.Content.ReadFromJsonAsync<Rental>();
                return ApiResult<Rental>.Success(createdRental);
            }
            catch (HttpRequestException e)
            {
                int statusCode = (int?)e.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Rental>.Failure($"Error de red: {e.Message}", statusCode);
            }
            catch (Exception e)
            {
                return ApiResult<Rental>.Failure($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdateRentalAsync(Rental rental)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/rentals/{rental.Id}", rental);
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

        public async Task<ApiResult<bool>> UpdateRentalAsync(RentalForUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/rentals/{dto.Id}", dto);
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

        public async Task<ApiResult<bool>> DeleteRentalAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/rentals/{id}");
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

        //public async Task<ApiResult> FinalizeRentalAsync(int rentalId, DateTime actualReturnDate)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync($"api/rentals/{rentalId}/finalize", actualReturnDate);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            return ApiResult.Success();
        //        }
        //        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        {
        //            return ApiResult.Failure($"Alquiler con ID {rentalId} no encontrado para finalizar.", (int)response.StatusCode);
        //        }
        //        else
        //        {
        //            var errorMessage = await response.Content.ReadAsStringAsync();
        //            return ApiResult.Failure($"Error al finalizar alquiler: {errorMessage}", (int)response.StatusCode);
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return ApiResult.Failure($"Error de red al finalizar alquiler: {ex.Message}");
        //    }
        //}

        // ✅ MÉTODO CORREGIDO para generar PDF
        public async Task<ApiResult> GenerateRentalContractPdfAsync(int rentalId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Generando contrato para rental ID: {rentalId}");

                // ✅ URL CORREGIDA: api/Rentals/generate-contract/{id}
                var response = await _httpClient.GetAsync($"api/Rentals/generate-contract/{rentalId}");

                Console.WriteLine($"[DEBUG] Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"[DEBUG] PDF recibido. Tamaño: {pdfBytes.Length} bytes");

                    // Obtener el nombre del archivo del header Content-Disposition
                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var fileName = contentDisposition?.FileName?.Trim('"') ?? $"Contrato_{rentalId}_{DateTime.Now:yyyyMMdd}.pdf";

                    Console.WriteLine($"[DEBUG] Nombre del archivo: {fileName}");

                    // ✅ SOLUCIÓN: Usar downloadFileFromByteArray en lugar de convertir a base64
                    await _jsRuntime.InvokeVoidAsync("downloadFileFromByteArray", fileName, pdfBytes);

                    Console.WriteLine("[DEBUG] Archivo descargado exitosamente");

                    return ApiResult.Success("Contrato PDF generado y descargado exitosamente.");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] Error del servidor: {errorMessage}");
                    return ApiResult.Failure($"Error al generar el contrato PDF: {errorMessage}", (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                return ApiResult.Failure($"Error de red al generar el contrato PDF: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<VehicleFinancialReportDto>>> GetVehicleFinancialReportAsync(DateTime startDate, DateTime endDate, int vehicleId = 0)
        {
            try
            {
                var start = startDate.ToString("yyyy-MM-dd");
                var end = endDate.ToString("yyyy-MM-dd");

                var url = $"api/rentals/reports/vehicle?startDate={start}&endDate={end}";

                if (vehicleId > 0)
                {
                    url += $"&vehicleId={vehicleId}";
                }

                Console.WriteLine($"[DEBUG] RentalApiClient - GetVehicleFinancialReport URL: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var report = await response.Content.ReadFromJsonAsync<List<VehicleFinancialReportDto>>();
                    Console.WriteLine($"[DEBUG] RentalApiClient - Success, received {report?.Count ?? 0} vehicle reports");
                    return ApiResult<List<VehicleFinancialReportDto>>.Success(report ?? new List<VehicleFinancialReportDto>());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] RentalApiClient - Status: {response.StatusCode}, Error: {error}");
                    return ApiResult<List<VehicleFinancialReportDto>>.Failure($"Error al obtener el reporte financiero: {response.StatusCode} - {error}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int?)ex.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"[ERROR] RentalApiClient - HttpRequestException: {ex.Message}");
                return ApiResult<List<VehicleFinancialReportDto>>.Failure($"Error de red al obtener el reporte financiero: {ex.Message}", statusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RentalApiClient - Exception: {ex.Message}");
                return ApiResult<List<VehicleFinancialReportDto>>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<MonthlyFinancialReportDto>>> GetMonthlyFinancialReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.ToString("yyyy-MM-dd");
                var end = endDate.ToString("yyyy-MM-dd");

                var url = $"api/rentals/reports/monthly?startDate={start}&endDate={end}";

                Console.WriteLine($"[DEBUG] RentalApiClient - GetMonthlyFinancialReport URL: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var report = await response.Content.ReadFromJsonAsync<List<MonthlyFinancialReportDto>>();
                    Console.WriteLine($"[DEBUG] RentalApiClient - Success, received {report?.Count ?? 0} monthly reports");
                    return ApiResult<List<MonthlyFinancialReportDto>>.Success(report ?? new List<MonthlyFinancialReportDto>());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] RentalApiClient - Monthly Status: {response.StatusCode}, Error: {error}");
                    return ApiResult<List<MonthlyFinancialReportDto>>.Failure($"Error al obtener el reporte mensual: {response.StatusCode} - {error}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int?)ex.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"[ERROR] RentalApiClient - Monthly HttpRequestException: {ex.Message}");
                return ApiResult<List<MonthlyFinancialReportDto>>.Failure($"Error de red al obtener el reporte mensual: {ex.Message}", statusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RentalApiClient - Monthly Exception: {ex.Message}");
                return ApiResult<List<MonthlyFinancialReportDto>>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResult<List<ProfitReportDto>>> GetProfitabilityReportAsync(DateTime startDate, DateTime endDate, int vehicleId = 0)
        {
            try
            {
                var start = startDate.ToString("yyyy-MM-dd");
                var end = endDate.ToString("yyyy-MM-dd");

                var url = $"api/rentals/reports/profitability?startDate={start}&endDate={end}";

                if (vehicleId > 0)
                {
                    url += $"&vehicleId={vehicleId}";
                }

                Console.WriteLine($"[DEBUG] RentalApiClient - GetProfitabilityReport URL: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var report = await response.Content.ReadFromJsonAsync<List<ProfitReportDto>>();
                    Console.WriteLine($"[DEBUG] RentalApiClient - Profitability Success, received {report?.Count ?? 0} profit reports");
                    return ApiResult<List<ProfitReportDto>>.Success(report ?? new List<ProfitReportDto>());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] RentalApiClient - Profitability Status: {response.StatusCode}, Error: {error}");
                    return ApiResult<List<ProfitReportDto>>.Failure($"Error al obtener el reporte de rentabilidad: {response.StatusCode} - {error}", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int?)ex.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"[ERROR] RentalApiClient - Profitability HttpRequestException: {ex.Message}");
                return ApiResult<List<ProfitReportDto>>.Failure($"Error de red al obtener el reporte de rentabilidad: {ex.Message}", statusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RentalApiClient - Profitability Exception: {ex.Message}");
                return ApiResult<List<ProfitReportDto>>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        // CarRental.Web/Services/RentalApiClient.cs - AGREGAR ESTE MÉTODO

        // Agregar este método a la clase RentalApiClient existente

        public async Task<ApiResult<CancelRentalResponse>> CancelRentalAsync(int rentalId, bool calculateDays)
        {
            try
            {
                var request = new { CalculateDays = calculateDays };
                var response = await _httpClient.PostAsJsonAsync($"api/Rentals/{rentalId}/cancel", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CancelRentalResponse>();
                    return ApiResult<CancelRentalResponse>.Success(result, result?.Message ?? "Alquiler cancelado exitosamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<CancelRentalResponse>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<CancelRentalResponse>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public async Task<ApiResult<Rental>> FinalizeRentalAsync(int rentalId, DateTime actualReturnDate)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Rentals/{rentalId}/finalize", null);

                if (response.IsSuccessStatusCode)
                {
                    var rental = await response.Content.ReadFromJsonAsync<Rental>();
                    return ApiResult<Rental>.Success(rental, "Alquiler finalizado exitosamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<Rental>.Failure($"Error: {response.StatusCode} - {errorMessage}", (int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int?)ex.StatusCode ?? (int)HttpStatusCode.InternalServerError;
                return ApiResult<Rental>.Failure($"Error de red: {ex.Message}", statusCode);
            }
            catch (Exception ex)
            {
                return ApiResult<Rental>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public async Task<ApiResult<object>> UpdateRentalStatusesAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/Rentals/update-statuses", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<object>();
                    return ApiResult<object>.Success(result, "Estados actualizados correctamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<object>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<object>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public class CancelRentalResponse
        {
            public string Message { get; set; }
            public int RentalId { get; set; }
            public decimal FinalCost { get; set; }
            public bool CalculatedDays { get; set; }
        }
    }
}