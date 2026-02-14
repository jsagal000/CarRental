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
            var maintenance = new Maintenance { VehicleId = vehicleId, Date = maintenanceDto.Date, Description = maintenanceDto.Description, Cost = maintenanceDto.Cost, WorkshopName = maintenanceDto.WorkshopName, Mileage = maintenanceDto.Mileage };
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

        // Aquí irían los métodos para PUT y DELETE, siguiendo el mismo patrón...
    }
}