// CarRental.Infrastructure/Services/RentalService.cs
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using CarRental.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IPaymentRepository _paymentRepository;
        public RentalService(
            IRentalRepository rentalRepository,
            IVehicleRepository vehicleRepository,
            ILogger<RentalService> logger,
            CarRentalDbContext context,
            IPdfGeneratorService pdfGeneratorService,
            ICompanySettingsService companySettingsService,
            ContractNumberService contractNumberService,
            IPaymentRepository paymentRepository)
        {
            _rentalRepository = rentalRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _context = context;
            _pdfGeneratorService = pdfGeneratorService;
            _companySettingsService = companySettingsService;
            _contractNumberService = contractNumberService;
            _paymentRepository = paymentRepository;
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

        // ============================================================================
        // ✅ MÉTODO CORREGIDO: FinalizeRentalAsync
        // ============================================================================
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

                // ✅ VALIDACIÓN 1: Solo se puede finalizar desde Activo o Vencido
                if (rental.Status != Rental.RentalStatus.Activo && rental.Status != Rental.RentalStatus.Vencido)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no puede finalizarse desde estado {RentalStatus}.", rentalId, rental.Status);
                    throw new InvalidOperationException($"El alquiler no puede finalizarse desde estado {rental.Status}. Solo se puede finalizar desde Activo o Vencido.");
                }

                // ============================================================================
                // ✅ VALIDACIÓN 2: VERIFICAR QUE EXISTA PAGO
                // ============================================================================
                var payments = await _paymentRepository.GetPaymentsByRentalIdAsync(rentalId);
                if (payments == null || !payments.Any())
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no tiene pagos registrados para finalizar.", rentalId);
                    throw new InvalidOperationException("No se puede finalizar el alquiler sin pagos registrados. Debe agregar al menos un pago.");
                }

                // ============================================================================
                // ✅ VALIDACIÓN 3: VERIFICAR QUE EL PAGO SEA IGUAL AL COSTO TOTAL
                // ============================================================================
                var totalPaid = await _paymentRepository.GetTotalPaidByRentalIdAsync(rentalId);

                rental.ActualReturnDate = actualReturnDate;

                // Calcular fecha/hora de devolución para comparación
                DateTime actualReturnDateTimeForCalc = actualReturnDate.Date.Add(rental.StartDate.TimeOfDay);
                if (actualReturnDateTimeForCalc < rental.StartDate)
                {
                    actualReturnDateTimeForCalc = rental.StartDate.AddDays(1);
                }

                // Calcular días reales de alquiler
                TimeSpan actualDuration = actualReturnDateTimeForCalc - rental.StartDate;
                int actualRentalDays = (int)Math.Ceiling(actualDuration.TotalHours / 24.0);
                if (actualRentalDays <= 0) actualRentalDays = 1;

                decimal newTotalCost = actualRentalDays * rental.DailyRate;

                // ✅ CALCULAR RECARGOS SI DEVUELVE DESPUÉS DE LA FECHA FIN
                if (actualReturnDateTimeForCalc > rental.EndDate)
                {
                    TimeSpan overdueDuration = actualReturnDateTimeForCalc - rental.EndDate;
                    int overdueDays = (int)Math.Ceiling(overdueDuration.TotalHours / 24.0);
                    if (overdueDays < 0) overdueDays = 0;

                    decimal overdueChargePerDay = rental.DailyRate * 1.5m;
                    rental.OverdueCharges = overdueDays * overdueChargePerDay;
                    newTotalCost += rental.OverdueCharges;

                    _logger.LogInformation("Alquiler con ID {RentalId} finalizado con {OverdueDays} días de retraso. Cargos adicionales: {OverdueCharges}.",
                        rentalId, overdueDays, rental.OverdueCharges);
                }
                else
                {
                    rental.OverdueCharges = 0m;
                    _logger.LogInformation("Alquiler con ID {RentalId} finalizado a tiempo (sin recargos).", rentalId);
                }

                rental.TotalCost = newTotalCost;

                // ============================================================================
                // ✅ VALIDACIÓN 4: VERIFICAR QUE EL PAGO SEA IGUAL AL COSTO TOTAL
                // ============================================================================
                if (totalPaid != rental.TotalCost)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} pago insuficiente. Pagado: {TotalPaid}, Requerido: {TotalCost}",
                        rentalId, totalPaid, rental.TotalCost);
                    throw new InvalidOperationException(
                        $"El monto total pagado ({totalPaid:C}) no coincide con el costo total del alquiler ({rental.TotalCost:C}). " +
                        $"Debe pagar exactamente el monto total antes de finalizar.");
                }

                // ============================================================================
                // ✅ MEJORA: ACTUALIZAR ENDDATE CUANDO SE FINALIZA UN ALQUILER VENCIDO
                // ============================================================================
                if (rental.Status == Rental.RentalStatus.Vencido)
                {
                    // Actualizar EndDate a la fecha real de devolución
                    rental.EndDate = actualReturnDate;
                    _logger.LogInformation("Alquiler {RentalId} era Vencido. EndDate actualizado a fecha real de devolución: {ActualReturnDate}",
                        rentalId, actualReturnDate);
                }

                // ✅ SIEMPRE FINALIZAR COMO COMPLETADO (independiente de si hubo retraso o venía de Vencido)
                rental.Status = Rental.RentalStatus.Completado;

                await _rentalRepository.UpdateAsync(rental);
                _logger.LogInformation("Alquiler con ID {RentalId} finalizado exitosamente. Estado: COMPLETADO, Costo Total: {TotalCost}, Pagado: {TotalPaid}",
                    rentalId, rental.TotalCost, totalPaid);

                // ✅ SIEMPRE cambiar vehículo a Disponible al finalizar
                var vehicle = await _vehicleRepository.GetByIdAsync(rental.VehicleId);
                if (vehicle != null)
                {
                    vehicle.State = Vehicle.VehicleState.Disponible;
                    await _vehicleRepository.UpdateAsync(vehicle);
                    _logger.LogInformation("Estado del vehículo con ID {VehicleId} actualizado a DISPONIBLE después de finalizar alquiler.", vehicle.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar el alquiler con ID {RentalId}.", rentalId);
                throw;
            }
        }

        public async Task CancelRentalAsync(int rentalId, decimal cancellationAmount = 0)
        {
            _logger.LogInformation("Iniciando cancelación del alquiler con ID {RentalId}. Monto de cancelación: {CancellationAmount}", rentalId, cancellationAmount);
            try
            {
                var rental = await _rentalRepository.GetRentalWithDetailsByIdAsync(rentalId);
                if (rental == null)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no encontrado para la cancelación.", rentalId);
                    throw new ArgumentException("Alquiler no encontrado.");
                }

                // ✅ VALIDACIÓN 1: Solo se puede cancelar desde Reservado o Activo
                if (rental.Status != Rental.RentalStatus.Reservado && rental.Status != Rental.RentalStatus.Activo)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no puede cancelarse desde estado {RentalStatus}.", rentalId, rental.Status);
                    throw new InvalidOperationException($"Solo se pueden cancelar alquileres en estado Reservado o Activo. Estado actual: {rental.Status}");
                }

                // ============================================================================
                // ✅ VALIDACIÓN 2: Si hay monto de cancelación, debe haber pago registrado
                // ============================================================================
                if (cancellationAmount > 0)
                {
                    var payments = await _paymentRepository.GetPaymentsByRentalIdAsync(rentalId);
                    if (payments == null || !payments.Any())
                    {
                        _logger.LogWarning("Alquiler con ID {RentalId} intenta cancelarse con monto pero sin pagos registrados.", rentalId);
                        throw new InvalidOperationException(
                            "No se puede cancelar el alquiler con monto de cancelación sin pago registrado. " +
                            "Debe registrar el pago de cancelación antes de cancelar.");
                    }

                    // ✅ VALIDACIÓN 3: El pago debe ser igual al monto de cancelación
                    var totalPaid = await _paymentRepository.GetTotalPaidByRentalIdAsync(rentalId);
                    if (totalPaid != cancellationAmount)
                    {
                        _logger.LogWarning("Alquiler con ID {RentalId} cancelación con monto incorrecto. Pagado: {TotalPaid}, Requerido: {CancellationAmount}",
                            rentalId, totalPaid, cancellationAmount);
                        throw new InvalidOperationException(
                            $"El monto pagado ({totalPaid:C}) no coincide con el monto de cancelación ({cancellationAmount:C}). " +
                            $"Debe pagar exactamente el monto de cancelación.");
                    }
                }

                // Cambiar estado a Cancelado
                rental.Status = Rental.RentalStatus.Cancelado;
                rental.ActualReturnDate = DateTime.UtcNow;

                await _rentalRepository.UpdateAsync(rental);
                _logger.LogInformation("Alquiler con ID {RentalId} cancelado exitosamente. Monto de cancelación: {CancellationAmount}",
                    rentalId, cancellationAmount);

                // ✅ Cambiar vehículo a Disponible al cancelar
                var vehicle = await _vehicleRepository.GetByIdAsync(rental.VehicleId);
                if (vehicle != null)
                {
                    vehicle.State = Vehicle.VehicleState.Disponible;
                    await _vehicleRepository.UpdateAsync(vehicle);
                    _logger.LogInformation("Estado del vehículo con ID {VehicleId} actualizado a DISPONIBLE después de cancelar alquiler.", vehicle.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar el alquiler con ID {RentalId}.", rentalId);
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
