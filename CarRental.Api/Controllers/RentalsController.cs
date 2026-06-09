using CarRental.Api.Attributes;
using CarRental.Api.Extensions;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RentalsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly IRentalService _rentalService;
        private readonly RentalStatusUpdateService _rentalStatusUpdateService;
        private readonly IAuditService _auditService;
        private readonly ILogger<RentalsController> _logger;

        public RentalsController(
            IRentalService rentalService,
            ILogger<RentalsController> logger,
            CarRentalDbContext context,
            IAuditService auditService,
            RentalStatusUpdateService rentalStatusUpdateService)
        {
            _rentalService = rentalService;
            _logger = logger;
            _context = context;
            _auditService = auditService;
            _rentalStatusUpdateService = rentalStatusUpdateService;
        }

        [HttpGet]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<IEnumerable<Rental>>> GetRentals()
        {
            var userId = GetCurrentUserId();

            try
            {
                var rentals = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Include(r => r.Customer)
                    .ToListAsync();

                return rentals;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "View",
                    description: "Error al consultar lista de alquileres",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id:int}")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<Rental>> GetRental(int id)
        {
            var userId = GetCurrentUserId();

            if (id <= 0)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "View",
                    entityId: id,
                    description: "Error de validación: ID inválido",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: "ID inválido"
                );

                return BadRequest("Invalid rental ID");
            }

            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rental == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "View",
                        entityId: id,
                        description: $"Intentó consultar alquiler inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Alquiler no encontrado"
                    );

                    return NotFound($"Rental with ID {id} not found");
                }

                return rental;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "View",
                    entityId: id,
                    description: $"Error al consultar alquiler (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error retrieving rental with ID {RentalId}", id);
                return StatusCode(500, "An error occurred while retrieving the rental");
            }
        }

        [HttpPost]
        [RequirePermission("Rental", "Create")]
        public async Task<ActionResult<Rental>> PostRental(RentalForCreationDto dto)
        {
            var userId = GetCurrentUserId();

            try
            {
                var startDate = dto.StartDate;
                var endDate = dto.EndDate;

                if (!string.IsNullOrEmpty(dto.StartTimeInput) && TimeSpan.TryParse(dto.StartTimeInput, out TimeSpan startTime))
                {
                    startDate = startDate.Date.Add(startTime);
                    endDate = endDate.Date.Add(startTime);
                }

                var duration = endDate - startDate;
                var totalDays = Math.Max(1, (int)Math.Ceiling(duration.TotalDays));
                var totalCost = totalDays * dto.DailyRate;
                var initialStatus = DetermineRentalStatus(startDate);

                var rental = new Rental
                {
                    CustomerId = dto.CustomerId,
                    VehicleId = dto.VehicleId,
                    StartDate = startDate,
                    EndDate = endDate,
                    DailyRate = dto.DailyRate,
                    TotalCost = totalCost,
                    DestinationType = dto.DestinationType,
                    DestinationCityName = dto.DestinationCityName,
                    MileageAtDelivery = dto.MileageAtDelivery,
                    Notes = dto.Notes,
                    DriverName = dto.DriverName,
                    DriverLicenseType = dto.DriverLicenseType,
                    DriverLicenseExpirationDate = dto.DriverLicenseExpirationDate,
                    Status = initialStatus
                };

                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();

                await UpdateVehicleStatus(dto.VehicleId, initialStatus);

                // ✅ AUDITAR: Creación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Create",
                    entityId: rental.Id,
                    entityName: $"Alquiler {dto.DriverName} - Veh ID: {dto.VehicleId}",
                    description: $"Creó nuevo alquiler para {dto.DriverName} del {startDate:yyyy-MM-dd} al {endDate:yyyy-MM-dd}",
                    newValues: new
                    {
                        rental.Id,
                        rental.CustomerId,
                        rental.VehicleId,
                        rental.StartDate,
                        rental.EndDate,
                        rental.DailyRate,
                        rental.TotalCost,
                        rental.DestinationType,
                        rental.DestinationCityName,
                        rental.DriverName,
                        rental.Status
                    },
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Create",
                    description: $"Error al crear alquiler para {dto.DriverName}",
                    newValues: dto,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Rental", "Edit")]
        public async Task<IActionResult> PutRental(int id, RentalForUpdateDto dto)
        {
            var userId = GetCurrentUserId();

            if (id != dto.Id)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    entityId: id,
                    description: "Error de validación: ID no coincide",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: "ID no coincide"
                );

                return BadRequest("El ID de la URL no coincide con el ID del alquiler.");
            }

            try
            {
                var existingRental = await _context.Rentals.FindAsync(id);
                if (existingRental == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Edit",
                        entityId: id,
                        description: $"Intentó editar alquiler inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Alquiler no encontrado"
                    );

                    return NotFound();
                }

                var previousStatus = existingRental.Status;
                var previousVehicleId = existingRental.VehicleId;

                // ✅ NUEVA LÓGICA: Si el alquiler está VENCIDO y se extiende la fecha fin
                if (existingRental.Status == Rental.RentalStatus.Vencido && dto.EndDate.Date > DateTime.Today)
                {
                    existingRental.Status = Rental.RentalStatus.Activo;

                    _logger.LogInformation(
                        "Alquiler {RentalId} cambiado de VENCIDO → ACTIVO (fecha extendida de {OldEndDate} a {NewEndDate})",
                        id, existingRental.EndDate, dto.EndDate
                    );
                }

                var oldValues = new
                {
                    existingRental.CustomerId,
                    existingRental.VehicleId,
                    existingRental.StartDate,
                    existingRental.EndDate,
                    existingRental.DailyRate,
                    existingRental.TotalCost,
                    existingRental.DestinationType,
                    existingRental.DestinationCityName,
                    existingRental.DriverName,
                    Status = previousStatus  // ✅ Usar previousStatus para guardar el estado original
                };

                existingRental.CustomerId = dto.CustomerId;
                existingRental.VehicleId = dto.VehicleId;
                existingRental.StartDate = dto.StartDate;
                existingRental.EndDate = dto.EndDate;
                existingRental.DailyRate = dto.DailyRate;
                existingRental.DestinationType = dto.DestinationType;
                existingRental.DestinationCityName = dto.DestinationCityName;
                existingRental.MileageAtDelivery = dto.MileageAtDelivery;
                existingRental.Notes = dto.Notes;
                existingRental.DriverName = dto.DriverName;
                existingRental.DriverLicenseType = dto.DriverLicenseType;
                existingRental.DriverLicenseExpirationDate = dto.DriverLicenseExpirationDate;
                existingRental.TotalCost = dto.TotalCost;

                // ✅ Solo actualizar Status si NO fue reactivado manualmente desde Vencido
                if (existingRental.Status != Rental.RentalStatus.Activo || previousStatus != Rental.RentalStatus.Vencido)
                {
                    existingRental.Status = DetermineRentalStatus(dto.StartDate);
                }

                await _context.SaveChangesAsync();

                if (previousVehicleId != dto.VehicleId)
                {
                    await UpdateVehicleStatus(previousVehicleId, Rental.RentalStatus.Completado);
                }

                await UpdateVehicleStatus(dto.VehicleId, existingRental.Status);

                var newValues = new
                {
                    existingRental.CustomerId,
                    existingRental.VehicleId,
                    existingRental.StartDate,
                    existingRental.EndDate,
                    existingRental.DailyRate,
                    existingRental.TotalCost,
                    existingRental.DestinationType,
                    existingRental.DestinationCityName,
                    existingRental.DriverName,
                    existingRental.Status
                };

                // ✅ AUDITAR: Edición de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    entityId: existingRental.Id,
                    entityName: $"Alquiler {dto.DriverName} - Veh ID: {dto.VehicleId}",
                    description: $"Editó alquiler ID: {existingRental.Id}" +
                                (previousStatus == Rental.RentalStatus.Vencido && existingRental.Status == Rental.RentalStatus.Activo
                                    ? " (Extendido desde Vencido → Activo)"
                                    : ""),
                    oldValues: oldValues,
                    newValues: newValues,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RentalExists(id))
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Edit",
                        entityId: id,
                        description: $"Error de concurrencia: alquiler no encontrado (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Alquiler no encontrado"
                    );

                    return NotFound();
                }
                else
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Edit",
                        entityId: id,
                        description: $"Error de concurrencia al editar alquiler (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Error de concurrencia"
                    );

                    throw;
                }
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    entityId: id,
                    description: $"Error al editar alquiler (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpDelete("{id}")]
        [RequirePermission("Rental", "Delete")]
        public async Task<IActionResult> DeleteRental(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var rental = await _context.Rentals.FindAsync(id);
                if (rental == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Delete",
                        entityId: id,
                        description: $"Intentó eliminar alquiler inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Alquiler no encontrado"
                    );

                    return NotFound();
                }

                var deletedRentalData = new
                {
                    rental.Id,
                    rental.CustomerId,
                    rental.VehicleId,
                    rental.StartDate,
                    rental.EndDate,
                    rental.DailyRate,
                    rental.TotalCost,
                    rental.DestinationType,
                    rental.DestinationCityName,
                    rental.DriverName,
                    rental.Status
                };

                await UpdateVehicleStatus(rental.VehicleId, Rental.RentalStatus.Completado);

                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Eliminación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Delete",
                    entityId: rental.Id,
                    entityName: $"Alquiler {rental.DriverName} - Veh ID: {rental.VehicleId}",
                    description: $"Eliminó alquiler ID: {rental.Id} del conductor {rental.DriverName}",
                    oldValues: deletedRentalData,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Delete",
                    entityId: id,
                    description: $"Error al eliminar alquiler (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost("{id}/finalize")]
        [RequirePermission("Rental", "Edit")]
        public async Task<ActionResult<ApiResult<Rental>>> FinalizeRental(int id)
        {
            var userId = GetCurrentUserId();
            var userName = User.Identity?.Name ?? "Sistema";

            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);

                if (rental == null)
                {
                    return NotFound(ApiResult<Rental>.Failure($"Alquiler con ID {id} no encontrado"));
                }

                // ✅ CORREGIDO: Permitir finalizar si está Activo o Vencido
                if (rental.Status != Rental.RentalStatus.Activo && rental.Status != Rental.RentalStatus.Vencido)
                {
                    return BadRequest(ApiResult<Rental>.Failure($"Solo se pueden finalizar alquileres en estado Activo o Vencido. Estado actual: {rental.Status}"));
                }

                // ✅ Cambiar estado a Finalizado (incluso si estaba Vencido)
                rental.Status = Rental.RentalStatus.Completado;
                rental.ActualReturnDate = DateTime.UtcNow;

                await _rentalService.FinalizeRentalAsync(id, DateTime.UtcNow);

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Finalize",
                    entityId: id,
                    entityName: $"Alquiler {id}",
                    description: $"Finalizó el alquiler ID {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return Ok(ApiResult<Rental>.Success(rental, "Alquiler finalizado exitosamente"));
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Finalize",
                    entityId: id,
                    description: $"Error al finalizar alquiler ID {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error al finalizar alquiler {RentalId}", id);
                return StatusCode(500, ApiResult<Rental>.Failure($"Error al finalizar el alquiler: {ex.Message}"));
            }
        }


        [HttpGet("reports/monthly")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<List<MonthlyFinancialReportDto>>> GetMonthlyFinancialReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetCurrentUserId();

            try
            {
                _logger.LogInformation("Getting monthly financial report from {StartDate} to {EndDate}", startDate, endDate);

                var rentals = await _context.Rentals
                    .Where(r => r.StartDate >= startDate && r.StartDate <= endDate)
                    .Where(r => r.Status == Rental.RentalStatus.Completado || r.Status == Rental.RentalStatus.Activo)
                    .ToListAsync();

                var monthlyReport = rentals
                    .GroupBy(r => new { r.StartDate.Year, r.StartDate.Month })
                    .Select(g => new MonthlyFinancialReportDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        TotalRevenue = g.Sum(r => r.TotalCost),
                        NumberOfRentals = g.Count(),
                        RentalCount = g.Count()
                    })
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .ToList();

                // Auditar generación de reporte
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: $"Generó reporte mensual financiero ({startDate:yyyy-MM-dd} a {endDate:yyyy-MM-dd})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                _logger.LogInformation("Generated monthly report with {Count} entries", monthlyReport.Count);
                return Ok(monthlyReport);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: "Error al generar reporte mensual financiero",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error generating monthly financial report");
                return StatusCode(500, "An error occurred while generating the monthly report");
            }
        }

        [HttpGet("reports/vehicle")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<List<VehicleFinancialReportDto>>> GetVehicleFinancialReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int vehicleId = 0)
        {
            var userId = GetCurrentUserId();

            try
            {
                _logger.LogInformation("Getting vehicle financial report from {StartDate} to {EndDate} for vehicle {VehicleId}",
                    startDate, endDate, vehicleId);

                var query = _context.Rentals
                    .Include(r => r.Vehicle)
                    .Where(r => r.StartDate >= startDate && r.StartDate <= endDate)
                    .Where(r => r.Status == Rental.RentalStatus.Completado || r.Status == Rental.RentalStatus.Activo);

                if (vehicleId > 0)
                {
                    query = query.Where(r => r.VehicleId == vehicleId);
                }

                var rentals = await query.ToListAsync();

                var vehicleReport = rentals
                    .GroupBy(r => r.Vehicle)
                    .Select(g => new VehicleFinancialReportDto
                    {
                        VehicleId = g.Key.Id,
                        Make = g.Key.Make,
                        Model = g.Key.Model,
                        Year = g.Key.Year,
                        Type = g.Key.Type,
                        LicensePlate = g.Key.LicensePlate,
                        TotalRevenue = g.Sum(r => r.TotalCost),
                        IsCompanyVehicle = g.Key.Ownership == Vehicle.OwnershipType.Empresa
                    })
                    .ToList();

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: $"Generó reporte financiero por vehículo ({startDate:yyyy-MM-dd} a {endDate:yyyy-MM-dd})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                _logger.LogInformation("Generated vehicle report with {Count} vehicles", vehicleReport.Count);
                return Ok(vehicleReport);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: "Error al generar reporte financiero por vehículo",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error generating vehicle financial report");
                return StatusCode(500, "An error occurred while generating the vehicle report");
            }
        }

        [HttpGet("reports/profitability")]
        [RequirePermission("Rental", "View")]
        public async Task<ActionResult<List<ProfitReportDto>>> GetProfitabilityReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int vehicleId = 0)
        {
            var userId = GetCurrentUserId();

            try
            {
                _logger.LogInformation("Getting profitability report from {StartDate} to {EndDate} for vehicle {VehicleId}",
                    startDate, endDate, vehicleId);

                var query = _context.Rentals
                    .Include(r => r.Vehicle)
                    .Where(r => r.StartDate >= startDate && r.StartDate <= endDate)
                    .Where(r => r.Status == Rental.RentalStatus.Completado || r.Status == Rental.RentalStatus.Activo);

                if (vehicleId > 0)
                {
                    query = query.Where(r => r.VehicleId == vehicleId);
                }

                var rentals = await query.ToListAsync();

                var profitReport = rentals
                    .GroupBy(r => r.Vehicle)
                    .Select(g => new ProfitReportDto
                    {
                        VehicleId = g.Key.Id,
                        TotalRevenue = g.Sum(r => r.TotalCost),
                        MaintenanceCost = 0,
                        NetProfit = g.Sum(r => r.TotalCost)
                    })
                    .ToList();

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: $"Generó reporte de rentabilidad ({startDate:yyyy-MM-dd} a {endDate:yyyy-MM-dd})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                _logger.LogInformation("Generated profitability report with {Count} vehicles", profitReport.Count);
                return Ok(profitReport);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    description: "Error al generar reporte de rentabilidad",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error generating profitability report");
                return StatusCode(500, "An error occurred while generating the profitability report");
            }
        }

        [HttpPost("update-statuses")]
        [RequirePermission("Rental", "Edit")]
        public async Task<IActionResult> UpdateRentalStatuses()
        {
            var userId = GetCurrentUserId();

            try
            {
                // Usar el servicio de actualización
                var updatedCount = await _rentalStatusUpdateService.UpdateRentalStatusesAsync();

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    description: $"Actualizó estados de {updatedCount} alquileres automáticamente",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                _logger.LogInformation("Updated {Count} rental statuses", updatedCount);
                return Ok(new { UpdatedRentals = updatedCount, Message = "Estados actualizados exitosamente" });
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    description: "Error al actualizar estados de alquileres",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error updating rental statuses");
                return StatusCode(500, "An error occurred while updating rental statuses");
            }
        }


        [HttpGet("generate-contract/{id}")]
        [RequirePermission("Rental", "View")]
        public async Task<IActionResult> GenerateContract(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var result = await _rentalService.GenerateRentalContractPdfAsync(id);

                if (!result.IsSuccess)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Export",
                        entityId: id,
                        description: $"Error al generar contrato PDF para alquiler ID: {id}",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: result.ErrorMessage
                    );

                    return BadRequest(new { message = result.ErrorMessage });
                }

                // Auditar generación exitosa
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    entityId: id,
                    description: $"Generó contrato PDF para alquiler ID: {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                // Obtener el rental para el nombre del archivo
                var rental = await _context.Rentals
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.Id == id);

                var fileName = rental != null
                    ? $"Contrato_{rental.Customer?.FirstName}_{rental.Customer?.LastName}_{DateTime.Now:yyyyMMdd}.pdf"
                    : $"Contrato_{id}_{DateTime.Now:yyyyMMdd}.pdf";

                // Devolver el PDF como descarga
                return File(result.Data, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Export",
                    entityId: id,
                    description: $"Error inesperado al generar contrato PDF para alquiler ID: {id}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error generating contract PDF for rental ID {RentalId}", id);
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        private Rental.RentalStatus DetermineRentalStatus(DateTime startDate)
        {
            var today = DateTime.Today;
            var startDay = startDate.Date;

            if (startDay == today)
            {
                return Rental.RentalStatus.Activo;
            }
            else if (startDay > today)
            {
                return Rental.RentalStatus.Reservado;
            }
            else
            {
                return Rental.RentalStatus.Activo;
            }
        }

        private async Task UpdateVehicleStatus(int vehicleId, Rental.RentalStatus rentalStatus)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle != null)
            {
                vehicle.State = rentalStatus switch
                {
                    Rental.RentalStatus.Reservado => Vehicle.VehicleState.Reservado,
                    Rental.RentalStatus.Activo => Vehicle.VehicleState.Alquilado,
                    _ => Vehicle.VehicleState.Disponible
                };

                await _context.SaveChangesAsync();
            }
        }

        private bool RentalExists(int id)
        {
            return (_context.Rentals?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }

        [HttpPost("{id}/cancel")]
        [RequirePermission("Rental", "Edit")]
        public async Task<IActionResult> CancelRental(int id, [FromBody] CancelRentalRequest request)
        {
            var userId = GetCurrentUserId();

            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Vehicle)
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rental == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Rental",
                        action: "Edit",
                        entityId: id,
                        description: $"Intentó cancelar alquiler inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Alquiler no encontrado"
                    );

                    return NotFound($"Rental with ID {id} not found");
                }

                // ✅ Validar que el alquiler pueda ser cancelado
                // NO se puede cancelar si ya está en estado final (Completado o Dañado)
                if (rental.Status == Rental.RentalStatus.Completado ||
                    rental.Status == Rental.RentalStatus.Dañado)
                {
                    return BadRequest("No se puede cancelar un alquiler que ya fue completado o marcado como dañado.");
                }

                if (rental.Status == Rental.RentalStatus.Cancelado)
                {
                    return BadRequest("El alquiler ya está cancelado.");
                }

                // ✅ PERMITIR cancelar desde: Reservado, Activo, Vencido

                // Si se solicita calcular los días
                if (request.CalculateDays)
                {
                    var now = DateTime.Now;
                    var actualReturnDate = now;

                    // Si el alquiler aún no ha comenzado, no se cobra nada
                    if (rental.StartDate > now)
                    {
                        rental.TotalCost = 0m;
                    }
                    else
                    {
                        // Calcular días transcurridos
                        var actualReturnDateTimeForCalc = actualReturnDate.Date.Add(rental.StartDate.TimeOfDay);
                        if (actualReturnDateTimeForCalc < rental.StartDate)
                        {
                            actualReturnDateTimeForCalc = rental.StartDate.AddDays(1);
                        }

                        TimeSpan actualDuration = actualReturnDateTimeForCalc - rental.StartDate;
                        int actualRentalDays = (int)Math.Ceiling(actualDuration.TotalHours / 24.0);
                        if (actualRentalDays <= 0) actualRentalDays = 1;

                        decimal newTotalCost = actualRentalDays * rental.DailyRate;

                        // ✅ Si está vencido, agregar recargos
                        if (rental.Status == Rental.RentalStatus.Vencido && actualReturnDateTimeForCalc > rental.EndDate)
                        {
                            TimeSpan overdueDuration = actualReturnDateTimeForCalc - rental.EndDate;
                            int overdueDays = (int)Math.Ceiling(overdueDuration.TotalHours / 24.0);
                            if (overdueDays > 0)
                            {
                                decimal overdueChargePerDay = rental.DailyRate * 1.5m;
                                decimal overdueCharges = overdueDays * overdueChargePerDay;
                                newTotalCost += overdueCharges;
                            }
                        }

                        rental.TotalCost = newTotalCost;
                    }

                    rental.ActualReturnDate = actualReturnDate;
                }
                else
                {
                    // No se calculan días, se cancela sin costo
                    rental.TotalCost = 0m;
                }

                rental.Status = Rental.RentalStatus.Cancelado;

                // Liberar el vehículo
                var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
                if (vehicle != null)
                {
                    vehicle.State = Vehicle.VehicleState.Disponible;
                }

                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    entityId: rental.Id,
                    entityName: $"Alquiler {rental.Customer?.FirstName} {rental.Customer?.LastName} - {rental.Vehicle?.Make} {rental.Vehicle?.Model}",
                    description: $"Canceló alquiler ID: {rental.Id} {(request.CalculateDays ? "con cálculo de días" : "sin cálculo de días")}. Costo final: ${rental.TotalCost}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                _logger.LogInformation("Rental {RentalId} cancelled. Calculate days: {CalculateDays}, Final cost: {TotalCost}",
                    rental.Id, request.CalculateDays, rental.TotalCost);

                return Ok(new
                {
                    Message = "Alquiler cancelado exitosamente",
                    RentalId = rental.Id,
                    FinalCost = rental.TotalCost,
                    CalculatedDays = request.CalculateDays
                });
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Rental",
                    action: "Edit",
                    entityId: id,
                    description: $"Error al cancelar alquiler (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error cancelling rental {RentalId}", id);
                return StatusCode(500, "An error occurred while cancelling the rental");
            }
        }

        // Clase para el request de cancelación
        public class CancelRentalRequest
        {
            public bool CalculateDays { get; set; }
        }
    }
}