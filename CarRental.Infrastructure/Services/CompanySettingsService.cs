using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Services
{
    public class CompanySettingsService : ICompanySettingsService
    {
        private readonly CarRentalDbContext _context;

        public CompanySettingsService(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResult<CompanySettings>> GetCompanySettingsAsync()
        {
            try
            {
                var settings = await _context.CompanySettings
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();

                if (settings == null)
                {
                    // Crear configuración por defecto
                    settings = new CompanySettings
                    {
                        CompanyName = "RENTADORA LUNA CAR RLC S.A.",
                        ActivityDescription = "ACTIVIDADES DE ALQUILER CON FINES OPERATIVOS\nDE AUTOMÓVILES DE PASAJEROS, CAMIONES, CAMIONETAS,\nREMOLQUES Y VEHÍCULOS DE RECREO",
                        Address = "Cdla. Simón Bolívar, Av. América Ms. 5 Sl. 100 Local 2",
                        City = "Guayaquil",
                        Phone1 = "0991581611",
                        Phone2 = "0999515414",
                        Email = "mildrecita1983@hotmail.com",
                        Website = "@rentacarluna",
                        CreatedDate = DateTime.Now
                    };

                    _context.CompanySettings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                return ApiResult<CompanySettings>.Success(settings);
            }
            catch (Exception ex)
            {
                return ApiResult<CompanySettings>.Failure($"Error al obtener configuración: {ex.Message}");
            }
        }

        public async Task<ApiResult<CompanySettings>> UpdateCompanySettingsAsync(CompanySettings settings)
        {
            try
            {
                settings.ModifiedDate = DateTime.Now;
                _context.CompanySettings.Update(settings);
                await _context.SaveChangesAsync();

                return ApiResult<CompanySettings>.Success(settings);
            }
            catch (Exception ex)
            {
                return ApiResult<CompanySettings>.Failure($"Error al actualizar configuración: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> UploadLogoAsync(byte[] logoData)
        {
            try
            {
                var settingsResult = await GetCompanySettingsAsync();
                if (!settingsResult.IsSuccess || settingsResult.Data == null)
                {
                    return ApiResult<bool>.Failure("No se encontró la configuración de la empresa");
                }

                var settings = settingsResult.Data;
                settings.Logo = logoData;
                settings.ModifiedDate = DateTime.Now;

                _context.CompanySettings.Update(settings);
                await _context.SaveChangesAsync();

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure($"Error al subir logo: {ex.Message}");
            }
        }
    }
}