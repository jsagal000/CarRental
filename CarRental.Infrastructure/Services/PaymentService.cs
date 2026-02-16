using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRentalRepository _rentalRepository;

        public PaymentService(IPaymentRepository paymentRepository, IRentalRepository rentalRepository)
        {
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            try
            {
                // Validar que el alquiler existe
                var rental = await _rentalRepository.GetByIdAsync(payment.RentalId);
                if (rental == null)
                {
                    throw new ArgumentException($"El alquiler con ID {payment.RentalId} no existe.");
                }

                // ✅ VALIDACIÓN NUEVA: Verificar que el pago no exceda el saldo pendiente
                var totalPaid = await GetTotalPaidByRentalIdAsync(payment.RentalId);
                var remainingBalance = rental.TotalCost - totalPaid;

                if (payment.Amount > remainingBalance)
                {
                    throw new ArgumentException(
                        $"El monto del pago (${payment.Amount:N2}) excede el saldo pendiente (${remainingBalance:N2}). " +
                        $"Costo total: ${rental.TotalCost:N2}, Ya pagado: ${totalPaid:N2}"
                    );
                }

                // Validar que el monto sea positivo
                if (payment.Amount <= 0)
                {
                    throw new ArgumentException("El monto del pago debe ser mayor que cero.");
                }

                // Establecer fecha de creación
                payment.CreatedDate = DateTime.UtcNow;

                // Agregar el pago
                await _paymentRepository.AddAsync(payment);

                return payment;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar pago para el alquiler {payment.RentalId}: {ex.Message}", ex);
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByRentalIdAsync(int rentalId)
        {
            return await _paymentRepository.GetPaymentsByRentalIdAsync(rentalId);
        }

        public async Task<decimal> GetTotalPaidByRentalIdAsync(int rentalId)
        {
            return await _paymentRepository.GetTotalPaidByRentalIdAsync(rentalId);
        }

        public async Task<decimal> GetRemainingBalanceAsync(int rentalId)
        {
            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental == null)
            {
                throw new ArgumentException($"El alquiler con ID {rentalId} no existe.");
            }

            var totalPaid = await GetTotalPaidByRentalIdAsync(rentalId);
            return rental.TotalCost - totalPaid;
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            // Validar que el pago existe
            var existingPayment = await _paymentRepository.GetByIdAsync(payment.Id);
            if (existingPayment == null)
            {
                throw new ArgumentException($"El pago con ID {payment.Id} no existe.");
            }

            // Validar que el alquiler existe
            var rental = await _rentalRepository.GetByIdAsync(payment.RentalId);
            if (rental == null)
            {
                throw new ArgumentException($"El alquiler con ID {payment.RentalId} no existe.");
            }

            // ✅ VALIDACIÓN NUEVA: Al actualizar, verificar que no exceda el saldo
            var totalPaid = await GetTotalPaidByRentalIdAsync(payment.RentalId);
            // Restar el monto del pago actual para calcular correctamente
            var otherPaymentsTotal = totalPaid - existingPayment.Amount;
            var remainingBalance = rental.TotalCost - otherPaymentsTotal;

            if (payment.Amount > remainingBalance)
            {
                throw new ArgumentException(
                    $"El monto del pago (${payment.Amount:N2}) excede el saldo disponible (${remainingBalance:N2})."
                );
            }

            // Validar que el monto sea positivo
            if (payment.Amount <= 0)
            {
                throw new ArgumentException("El monto del pago debe ser mayor que cero.");
            }

            await _paymentRepository.UpdateAsync(payment);
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new ArgumentException($"El pago con ID {id} no existe.");
            }

            await _paymentRepository.DeleteAsync(id);
        }
    }
}