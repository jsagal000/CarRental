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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly IAuditService _auditService;

        public VehiclesController(CarRentalDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        [RequirePermission("Vehicle", "View")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            var userId = GetCurrentUserId();

            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.Partner)
                    .ToListAsync();

                // NOTA: Ya no se auditan las consultas (View) según las mejores prácticas
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "View",
                    description: $"Consultó lista de vehículos ({vehicles.Count} registros)",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return vehicles;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "View",
                    description: "Error al consultar lista de vehículos",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("Vehicle", "View")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Partner)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Vehicle",
                        action: "View",
                        entityId: id,
                        description: $"Intentó consultar vehículo inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Vehículo no encontrado"
                    );

                    return NotFound();
                }

                // NOTA: Consulta exitosa - No se audita según mejores prácticas
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "View",
                    entityId: vehicle.Id,
                    entityName: $"{vehicle.Make} {vehicle.Model} - {vehicle.LicensePlate}",
                    description: $"Consultó detalles del vehículo {vehicle.Make} {vehicle.Model}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return vehicle;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "View",
                    entityId: id,
                    description: $"Error al consultar vehículo (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [RequirePermission("Vehicle", "Create")]
        public async Task<ActionResult<Vehicle>> PostVehicle(VehicleForCreationDto dto)
        {
            var userId = GetCurrentUserId();

            try
            {
                var vehicle = new Vehicle
                {
                    Make = dto.Make,
                    Model = dto.Model,
                    Type = dto.Type,
                    Year = dto.Year,
                    LicensePlate = dto.LicensePlate,
                    DailyRate = dto.DailyRate,
                    State = dto.State,
                    Vin = dto.Vin,
                    Color = dto.Color,
                    ImageUrls = dto.ImageUrls ?? new List<string>(),
                    Ownership = dto.Ownership,
                    PartnerId = dto.PartnerId
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Creación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Create",
                    entityId: vehicle.Id,
                    entityName: $"{vehicle.Make} {vehicle.Model} - {vehicle.LicensePlate}",
                    description: $"Creó nuevo vehículo: {vehicle.Make} {vehicle.Model} (Matrícula: {vehicle.LicensePlate})",
                    newValues: new
                    {
                        vehicle.Id,
                        vehicle.Make,
                        vehicle.Model,
                        vehicle.Type,
                        vehicle.Year,
                        vehicle.LicensePlate,
                        vehicle.DailyRate,
                        vehicle.State,
                        vehicle.Vin,
                        vehicle.Color,
                        vehicle.Ownership,
                        vehicle.PartnerId
                    },
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Create",
                    description: $"Error al crear vehículo: {dto.Make} {dto.Model}",
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
        [RequirePermission("Vehicle", "Edit")]
        public async Task<IActionResult> PutVehicle(int id, VehicleForUpdateDto dto)
        {
            var userId = GetCurrentUserId();

            if (id != dto.Id)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Edit",
                    entityId: id,
                    description: "Error de validación: ID no coincide",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: "ID no coincide"
                );

                return BadRequest("El ID de la URL no coincide con el ID del vehículo.");
            }

            try
            {
                var existingVehicle = await _context.Vehicles.FindAsync(id);
                if (existingVehicle == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Vehicle",
                        action: "Edit",
                        entityId: id,
                        description: $"Intentó editar vehículo inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Vehículo no encontrado"
                    );

                    return NotFound();
                }

                // Capturar valores antiguos para auditoría
                var oldValues = new
                {
                    existingVehicle.Make,
                    existingVehicle.Model,
                    existingVehicle.Type,
                    existingVehicle.Year,
                    existingVehicle.LicensePlate,
                    existingVehicle.DailyRate,
                    existingVehicle.State,
                    existingVehicle.Vin,
                    existingVehicle.Color,
                    existingVehicle.Ownership,
                    existingVehicle.PartnerId
                };

                // Actualizar campos
                existingVehicle.Make = dto.Make;
                existingVehicle.Model = dto.Model;
                existingVehicle.Type = dto.Type;
                existingVehicle.Year = dto.Year;
                existingVehicle.LicensePlate = dto.LicensePlate;
                existingVehicle.DailyRate = dto.DailyRate;
                existingVehicle.State = dto.State;
                existingVehicle.Vin = dto.Vin;
                existingVehicle.Color = dto.Color;
                existingVehicle.ImageUrls = dto.ImageUrls ?? new List<string>();
                existingVehicle.Ownership = dto.Ownership;
                existingVehicle.PartnerId = dto.PartnerId;

                await _context.SaveChangesAsync();

                var newValues = new
                {
                    existingVehicle.Make,
                    existingVehicle.Model,
                    existingVehicle.Type,
                    existingVehicle.Year,
                    existingVehicle.LicensePlate,
                    existingVehicle.DailyRate,
                    existingVehicle.State,
                    existingVehicle.Vin,
                    existingVehicle.Color,
                    existingVehicle.Ownership,
                    existingVehicle.PartnerId
                };

                // ✅ AUDITAR: Edición de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Edit",
                    entityId: existingVehicle.Id,
                    entityName: $"{existingVehicle.Make} {existingVehicle.Model} - {existingVehicle.LicensePlate}",
                    description: $"Editó vehículo: {existingVehicle.Make} {existingVehicle.Model}",
                    oldValues: oldValues,
                    newValues: newValues,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Vehicle",
                        action: "Edit",
                        entityId: id,
                        description: $"Error de concurrencia: vehículo no encontrado (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Vehículo no encontrado"
                    );

                    return NotFound();
                }
                else
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Vehicle",
                        action: "Edit",
                        entityId: id,
                        description: $"Error de concurrencia al editar vehículo (ID: {id})",
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
                    module: "Vehicle",
                    action: "Edit",
                    entityId: id,
                    description: $"Error al editar vehículo (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Vehicle", "Delete")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Vehicle",
                        action: "Delete",
                        entityId: id,
                        description: $"Intentó eliminar vehículo inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Vehículo no encontrado"
                    );

                    return NotFound();
                }

                // Capturar datos antes de eliminar
                var deletedVehicleData = new
                {
                    vehicle.Id,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Type,
                    vehicle.Year,
                    vehicle.LicensePlate,
                    vehicle.DailyRate,
                    vehicle.State,
                    vehicle.Vin,
                    vehicle.Color,
                    vehicle.Ownership,
                    vehicle.PartnerId
                };

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Eliminación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Delete",
                    entityId: vehicle.Id,
                    entityName: $"{vehicle.Make} {vehicle.Model} - {vehicle.LicensePlate}",
                    description: $"Eliminó vehículo: {vehicle.Make} {vehicle.Model} (Matrícula: {vehicle.LicensePlate})",
                    oldValues: deletedVehicleData,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Vehicle",
                    action: "Delete",
                    entityId: id,
                    description: $"Error al eliminar vehículo (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool VehicleExists(int id)
        {
            return (_context.Vehicles?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}