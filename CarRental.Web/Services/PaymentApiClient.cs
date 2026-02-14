// CarRental.Web/Services/PaymentApiClient.cs
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using System.Net.Http.Json;

namespace CarRental.Web.Services
{
    public class PaymentApiClient
    {
        private readonly AuthorizedHttpClient _httpClient;

        public PaymentApiClient(AuthorizedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<IEnumerable<Payment>>> GetPaymentsByRentalAsync(int rentalId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Payments/rental/{rentalId}");

                if (response.IsSuccessStatusCode)
                {
                    var payments = await response.Content.ReadFromJsonAsync<IEnumerable<Payment>>();
                    return ApiResult<IEnumerable<Payment>>.Success(payments);
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<IEnumerable<Payment>>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<IEnumerable<Payment>>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        //public async Task<ApiResult<Payment>> GetPaymentByIdAsync(int id)
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetAsync($"api/Payments/{id}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var payment = await response.Content.ReadFromJsonAsync<Payment>();
        //            return ApiResult<Payment>.Success(payment);
        //        }

        //        var errorMessage = await response.Content.ReadAsStringAsync();
        //        return ApiResult<Payment>.Failure($"Error: {response.StatusCode} - {errorMessage}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResult<Payment>.Failure($"Error de conexión: {ex.Message}");
        //    }
        //}

        public async Task<ApiResult<Payment>> GetPaymentByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/payments/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<Payment>();
                    return ApiResult<Payment>.Success(payment);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ApiResult<Payment>.Failure($"Error HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<Payment>.Failure($"Error al obtener el pago: {ex.Message}");
            }
        }
        public async Task<ApiResult<RentalBalance>> GetRentalBalanceAsync(int rentalId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Payments/rental/{rentalId}/balance");

                if (response.IsSuccessStatusCode)
                {
                    var balance = await response.Content.ReadFromJsonAsync<RentalBalance>();
                    return ApiResult<RentalBalance>.Success(balance);
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<RentalBalance>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<RentalBalance>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public async Task<ApiResult<Payment>> CreatePaymentAsync(PaymentForCreationDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Payments", dto);

                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<Payment>();
                    return ApiResult<Payment>.Success(payment, "Pago registrado exitosamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<Payment>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<Payment>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> UpdatePaymentAsync(int id, PaymentForUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Payments/{id}", dto);

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true, "Pago actualizado exitosamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<bool>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error de conexión: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> DeletePaymentAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Payments/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true, "Pago eliminado exitosamente");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                return ApiResult<bool>.Failure($"Error: {response.StatusCode} - {errorMessage}");
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error de conexión: {ex.Message}");
            }
        }

    }

    public class RentalBalance
    {
        public int RentalId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsFullyPaid { get; set; }
    }
}