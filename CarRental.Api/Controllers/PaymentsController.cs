using CarRental.Api.Attributes;
using CarRental.Api.Extensions;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IAuditService _auditService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            CarRentalDbContext context,
            IPaymentService paymentService,
            IAuditService auditService,
            ILogger<PaymentsController> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _auditService = auditService;
            _logger = logger;
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"View" en lugar de "Payment"/"View"
        [HttpGet("rental/{rentalId}")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByRental(int rentalId)
        {
            var userId = GetCurrentUserId();

            try
            {
                var payments = await _paymentService.GetPaymentsByRentalIdAsync(rentalId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "View",
                    description: $"Error al consultar pagos del alquiler {rentalId}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error retrieving payments for rental {RentalId}", rentalId);
                return StatusCode(500, "Error al obtener los pagos");
            }
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"View"
        [HttpGet("{id}")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Rental)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    return NotFound($"Pago con ID {id} no encontrado");
                }

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
                return StatusCode(500, "Error al obtener el pago");
            }
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"View"
        [HttpGet("rental/{rentalId}/balance")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<object>> GetRentalBalance(int rentalId)
        {
            try
            {
                var rental = await _context.Rentals.FindAsync(rentalId);
                if (rental == null)
                {
                    return NotFound($"Alquiler con ID {rentalId} no encontrado");
                }

                var totalPaid = await _paymentService.GetTotalPaidByRentalIdAsync(rentalId);
                var remainingBalance = await _paymentService.GetRemainingBalanceAsync(rentalId);

                return Ok(new
                {
                    RentalId = rentalId,
                    TotalCost = rental.TotalCost,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    IsFullyPaid = remainingBalance <= 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for rental {RentalId}", rentalId);
                return StatusCode(500, "Error al obtener el saldo");
            }
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"Edit" en lugar de "Payment"/"Edit"
        [HttpPost]
        [RequirePermission("Rental", "Edit")]
        public async Task<ActionResult<Payment>> PostPayment(PaymentForCreationDto dto)
        {
            var userId = GetCurrentUserId();
            var userName = User.Identity?.Name ?? "Sistema";

            try
            {
                // Logging detallado
                _logger.LogInformation("=== RECIBIENDO PAGO ===");
                _logger.LogInformation($"RentalId: {dto.RentalId}");
                _logger.LogInformation($"Amount: {dto.Amount}");
                _logger.LogInformation($"PaymentDate: {dto.PaymentDate}");
                _logger.LogInformation($"Method: {dto.Method}");
                _logger.LogInformation("====================");

                var payment = new Payment
                {
                    RentalId = dto.RentalId,
                    PaymentDate = dto.PaymentDate,
                    Amount = dto.Amount,
                    Method = dto.Method,
                    Notes = dto.Notes,
                    BankName = dto.BankName,
                    AccountType = dto.AccountType,
                    AccountNumber = dto.AccountNumber,
                    ReferenceNumber = dto.ReferenceNumber,
                    CreatedBy = userName
                };

                var createdPayment = await _paymentService.AddPaymentAsync(payment);

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Create",
                    entityId: createdPayment.Id,
                    entityName: $"Pago ${createdPayment.Amount}",
                    description: $"Registró pago de ${createdPayment.Amount} para alquiler {createdPayment.RentalId}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.Id }, createdPayment);
            }
            catch (ArgumentException ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Create",
                    description: "Error de validación al registrar pago",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Create",
                    description: "Error al registrar pago",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, "Error al crear el pago");
            }
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"Edit"
        [HttpPut("{id}")]
        [RequirePermission("Rental", "Edit")]
        public async Task<IActionResult> PutPayment(int id, PaymentForUpdateDto dto)
        {
            var userId = GetCurrentUserId();

            if (id != dto.Id)
            {
                return BadRequest("El ID del pago no coincide");
            }

            try
            {
                var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
                if (existingPayment == null)
                {
                    return NotFound($"Pago con ID {id} no encontrado");
                }

                existingPayment.RentalId = dto.RentalId;
                existingPayment.PaymentDate = dto.PaymentDate;
                existingPayment.Amount = dto.Amount;
                existingPayment.Method = dto.Method;
                existingPayment.Notes = dto.Notes;
                existingPayment.BankName = dto.BankName;
                existingPayment.AccountType = dto.AccountType;
                existingPayment.AccountNumber = dto.AccountNumber;
                existingPayment.ReferenceNumber = dto.ReferenceNumber;

                await _paymentService.UpdatePaymentAsync(existingPayment);

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Edit",
                    entityId: id,
                    entityName: $"Pago ${existingPayment.Amount}",
                    description: $"Actualizó pago ID {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Edit",
                    entityId: id,
                    description: "Error al actualizar pago",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error updating payment {PaymentId}", id);
                return StatusCode(500, "Error al actualizar el pago");
            }
        }

        // ✅ CAMBIADO: Usa permiso "Rental"/"Delete"
        [HttpDelete("{id}")]
        [RequirePermission("Rental", "Delete")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound($"Pago con ID {id} no encontrado");
                }

                await _paymentService.DeletePaymentAsync(id);

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Delete",
                    entityId: id,
                    entityName: $"Pago ${payment.Amount}",
                    description: $"Eliminó pago ID {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Payment",
                    action: "Delete",
                    entityId: id,
                    description: "Error al eliminar pago",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
                return StatusCode(500, "Error al eliminar el pago");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}