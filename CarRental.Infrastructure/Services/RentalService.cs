// CarRental.Infrastructure/Services/RentalService.cs
using CarRental.Core.Interfaces;
using CarRental.Infrastructure.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<RentalService> _logger;

        // ✅ NUEVAS dependencias para PDF
        private readonly CarRentalDbContext _context;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly ICompanySettingsService _companySettingsService;
        private readonly ContractNumberService _contractNumberService;

        public RentalService(
            IRentalRepository rentalRepository,
            IVehicleRepository vehicleRepository,
            ILogger<RentalService> logger,
            CarRentalDbContext context,
            IPdfGeneratorService pdfGeneratorService,
            ICompanySettingsService companySettingsService,
            ContractNumberService contractNumberService)
        {
            _rentalRepository = rentalRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _context = context;
            _pdfGeneratorService = pdfGeneratorService;
            _companySettingsService = companySettingsService;
            _contractNumberService = contractNumberService;
        }

        public async Task<IEnumerable<Rental>> GetAllRentalsAsync()
        {
            return await _rentalRepository.GetAllAsync();
        }

        public async Task<Rental> GetRentalByIdAsync(int id)
        {
            return await _rentalRepository.GetByIdAsync(id);
        }

        public async Task<Rental> AddRentalAsync(Rental rental)
        {
            await _rentalRepository.AddAsync(rental);
            return rental;
        }

        public async Task UpdateRentalAsync(Rental rental)
        {
            await _rentalRepository.UpdateAsync(rental);
        }

        public async Task DeleteRentalAsync(int id)
        {
            await _rentalRepository.DeleteAsync(id);
        }

        public async Task<decimal> CalculateRentalCostAsync(DateTime startDate, DateTime endDate, decimal dailyRate)
        {
            _logger.LogDebug("Calculando costo de alquiler desde {StartDate} hasta {EndDate} con tarifa diaria {DailyRate}.", startDate, endDate, dailyRate);
            try
            {
                decimal effectiveDailyRate = dailyRate;

                TimeSpan duration = endDate - startDate;
                int rentalDays = (int)Math.Ceiling(duration.TotalHours / 24.0);

                if (rentalDays <= 0)
                {
                    rentalDays = 1;
                }

                decimal cost = rentalDays * effectiveDailyRate;
                _logger.LogDebug("Costo de alquiler calculado: {Cost} por {RentalDays} días.", cost, rentalDays);
                return cost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el costo de alquiler con tarifa diaria {DailyRate}.", dailyRate);
                throw;
            }
        }

        public async Task FinalizeRentalAsync(int rentalId, DateTime actualReturnDate)
        {
            _logger.LogInformation("Iniciando la finalización del alquiler con ID {RentalId} en la fecha de devolución real {ActualReturnDate}.", rentalId, actualReturnDate);
            try
            {
                var rental = await _rentalRepository.GetRentalWithDetailsByIdAsync(rentalId);
                if (rental == null)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no encontrado para la finalización.", rentalId);
                    throw new ArgumentException("Renta no encontrada.");
                }

                if (rental.Status == Rental.RentalStatus.Completado || rental.Status == Rental.RentalStatus.Cancelado || rental.Status == Rental.RentalStatus.Dañado)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} ya está en un estado final ({RentalStatus}).", rentalId, rental.Status);
                    throw new InvalidOperationException("La renta ya ha sido finalizada o cancelada.");
                }

                rental.ActualReturnDate = actualReturnDate;
                rental.Status = Rental.RentalStatus.Completado;

                DateTime actualReturnDateTimeForCalc = actualReturnDate.Date.Add(rental.StartDate.TimeOfDay);

                if (actualReturnDateTimeForCalc < rental.StartDate)
                {
                    actualReturnDateTimeForCalc = rental.StartDate.AddDays(1);
                }

                TimeSpan actualDuration = actualReturnDateTimeForCalc - rental.StartDate;
                int actualRentalDays = (int)Math.Ceiling(actualDuration.TotalHours / 24.0);
                if (actualRentalDays <= 0) actualRentalDays = 1;

                decimal newTotalCost = actualRentalDays * rental.DailyRate;

                if (actualReturnDateTimeForCalc > rental.EndDate)
                {
                    TimeSpan overdueDuration = actualReturnDateTimeForCalc - rental.EndDate;
                    int overdueDays = (int)Math.Ceiling(overdueDuration.TotalHours / 24.0);
                    if (overdueDays < 0) overdueDays = 0;

                    decimal overdueChargePerDay = rental.DailyRate * 1.5m;
                    rental.OverdueCharges = overdueDays * overdueChargePerDay;
                    newTotalCost += rental.OverdueCharges;
                    rental.Status = Rental.RentalStatus.Vencido;
                    _logger.LogInformation("Alquiler con ID {RentalId} con retraso de {OverdueDays} días. Cargos por retraso: {OverdueCharges}.", rentalId, overdueDays, rental.OverdueCharges);
                }
                else
                {
                    rental.OverdueCharges = 0m;
                }

                rental.TotalCost = newTotalCost;

                await _rentalRepository.UpdateAsync(rental);
                _logger.LogInformation("Alquiler con ID {RentalId} finalizado exitosamente. Nuevo Costo Total: {TotalCost}", rentalId, rental.TotalCost);

                var vehicle = await _vehicleRepository.GetByIdAsync(rental.VehicleId);
                if (vehicle != null)
                {
                    vehicle.State = Vehicle.VehicleState.Disponible;
                    await _vehicleRepository.UpdateAsync(vehicle);
                    _logger.LogInformation("Estado del vehículo con ID {VehicleId} actualizado a {VehicleState} después de la finalización del alquiler.", vehicle.Id, Vehicle.VehicleState.Disponible);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar el alquiler con ID {RentalId}.", rentalId);
                throw;
            }
        }

        /// <summary>
        /// Generates a PDF contract document for a rental
        /// </summary>
        public async Task<ApiResult<byte[]>> GenerateRentalContractPdfAsync(int rentalId)
        {
            try
            {
                _logger.LogInformation("Iniciando generación de contrato PDF para alquiler ID {RentalId}", rentalId);

                // 1. Obtener el alquiler con todas las relaciones
                var rental = await _context.Rentals
                    .Include(r => r.Customer)
                    .Include(r => r.Vehicle)
                    .FirstOrDefaultAsync(r => r.Id == rentalId);

                if (rental == null)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no encontrado", rentalId);
                    return ApiResult<byte[]>.Failure("Alquiler no encontrado");
                }

                // 2. Obtener configuración de la empresa
                var companySettingsResult = await _companySettingsService.GetCompanySettingsAsync();
                if (!companySettingsResult.IsSuccess || companySettingsResult.Data == null)
                {
                    _logger.LogError("No se pudo obtener la configuración de la empresa");
                    return ApiResult<byte[]>.Failure("No se pudo obtener la configuración de la empresa");
                }

                // 3. Generar o recuperar número de contrato
                var contractNumber = await _contractNumberService.GenerateContractNumberAsync(rentalId);
                _logger.LogInformation("Número de contrato generado: {ContractNumber}", contractNumber);

                // 4. Generar el PDF
                var pdfBytes = await _pdfGeneratorService.GenerateRentalContractAsync(
                    rental,
                    companySettingsResult.Data,
                    contractNumber
                );

                // 5. Guardar el PDF en el servidor (opcional)
                try
                {
                    var uploadsPath = Path.Combine("wwwroot", "uploads", "contracts");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = $"Contrato_{contractNumber.Replace("-", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    await File.WriteAllBytesAsync(filePath, pdfBytes);
                    _logger.LogInformation("PDF guardado en {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo guardar el PDF en el servidor, pero se generó correctamente");
                }

                _logger.LogInformation("Contrato PDF generado exitosamente para alquiler ID {RentalId}", rentalId);
                return ApiResult<byte[]>.Success(pdfBytes, $"Contrato generado exitosamente: {contractNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar contrato PDF para alquiler ID {RentalId}", rentalId);
                return ApiResult<byte[]>.Failure($"Error al generar contrato: {ex.Message}");
            }
        }
    }
}