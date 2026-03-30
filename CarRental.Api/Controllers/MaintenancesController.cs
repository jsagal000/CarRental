using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class MaintenancesController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;
        public MaintenancesController(IMaintenanceService maintenanceService) { _maintenanceService = maintenanceService; }

        [HttpGet("vehicles/{vehicleId}/maintenances")]
        public async Task<IActionResult> GetMaintenancesByVehicle(int vehicleId) => Ok(await _maintenanceService.GetMaintenancesByVehicleIdAsync(vehicleId));

        [HttpPost("vehicles/{vehicleId}/maintenances")]
        public async Task<IActionResult> CreateMaintenance(int vehicleId, [FromBody] MaintenanceForCreationDto maintenanceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var maintenance = new Maintenance
            {
                VehicleId = vehicleId,
                // ============ CAMPOS ORIGINALES ============
                Date = maintenanceDto.Date,
                Description = maintenanceDto.Description,
                Cost = maintenanceDto.Cost,
                WorkshopName = maintenanceDto.WorkshopName,
                Mileage = maintenanceDto.Mileage,
                // ============ CAMPOS NUEVOS ============
                Type = maintenanceDto.Type,
                Category = maintenanceDto.Category,
                NextMaintenanceDate = maintenanceDto.NextMaintenanceDate,
                NextMaintenanceMileage = maintenanceDto.NextMaintenanceMileage,
                InvoiceNumber = maintenanceDto.InvoiceNumber,
                Notes = maintenanceDto.Notes
            };

            var newMaintenance = await _maintenanceService.AddMaintenanceAsync(maintenance);
            return CreatedAtAction(nameof(GetMaintenanceById), new { maintenanceId = newMaintenance.Id }, newMaintenance);
        }

        [HttpGet("maintenances/{maintenanceId}", Name = "GetMaintenanceById")]
        public async Task<IActionResult> GetMaintenanceById(int maintenanceId)
        {
            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
            if (maintenance == null) return NotFound();
            return Ok(maintenance);
        }

        [HttpPut("maintenances/{maintenanceId}")]
        public async Task<IActionResult> UpdateMaintenance(int maintenanceId, [FromBody] MaintenanceForCreationDto maintenanceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var maintenanceToUpdate = await _maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
            if (maintenanceToUpdate == null) return NotFound();

            // Mapear todos los campos del DTO al modelo
            // ============ CAMPOS ORIGINALES ============
            maintenanceToUpdate.Date = maintenanceDto.Date;
            maintenanceToUpdate.Description = maintenanceDto.Description;
            maintenanceToUpdate.Cost = maintenanceDto.Cost;
            maintenanceToUpdate.WorkshopName = maintenanceDto.WorkshopName;
            maintenanceToUpdate.Mileage = maintenanceDto.Mileage;
            // ============ CAMPOS NUEVOS ============
            maintenanceToUpdate.Type = maintenanceDto.Type;
            maintenanceToUpdate.Category = maintenanceDto.Category;
            maintenanceToUpdate.NextMaintenanceDate = maintenanceDto.NextMaintenanceDate;
            maintenanceToUpdate.NextMaintenanceMileage = maintenanceDto.NextMaintenanceMileage;
            maintenanceToUpdate.InvoiceNumber = maintenanceDto.InvoiceNumber;
            maintenanceToUpdate.Notes = maintenanceDto.Notes;

            await _maintenanceService.UpdateMaintenanceAsync(maintenanceToUpdate);
            return NoContent();
        }

        [HttpDelete("maintenances/{maintenanceId}")]
        public async Task<IActionResult> DeleteMaintenance(int maintenanceId)
        {
            await _maintenanceService.DeleteMaintenanceAsync(maintenanceId);
            return NoContent();
        }
    }
}
