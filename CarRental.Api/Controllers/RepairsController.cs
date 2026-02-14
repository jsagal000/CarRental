using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class RepairsController : ControllerBase
    {
        private readonly IRepairService _repairService;
        public RepairsController(IRepairService repairService) { _repairService = repairService; }

        [HttpGet("vehicles/{vehicleId}/repairs")]
        public async Task<IActionResult> GetRepairsByVehicle(int vehicleId) => Ok(await _repairService.GetRepairsByVehicleIdAsync(vehicleId));

        [HttpPost("vehicles/{vehicleId}/repairs")]
        public async Task<IActionResult> CreateRepair(int vehicleId, [FromBody] RepairForCreationDto repairDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var repair = new Repair { VehicleId = vehicleId, Date = repairDto.Date, Description = repairDto.Description, Cost = repairDto.Cost, WorkshopName = repairDto.WorkshopName, Mileage = repairDto.Mileage, IsWarranty = repairDto.IsWarranty };
            var newRepair = await _repairService.AddRepairAsync(repair);
            return CreatedAtAction(nameof(GetRepairById), new { repairId = newRepair.Id }, newRepair);
        }

        [HttpGet("repairs/{repairId}", Name = "GetRepairById")]
        public async Task<IActionResult> GetRepairById(int repairId)
        {
            var repair = await _repairService.GetRepairByIdAsync(repairId);
            if (repair == null) return NotFound();
            return Ok(repair);
        }

        // Aquí irían los métodos para PUT y DELETE, siguiendo el mismo patrón...
    }
}
