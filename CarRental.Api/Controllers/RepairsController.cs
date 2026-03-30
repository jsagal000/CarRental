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

            var repair = new Repair
            {
                VehicleId = vehicleId,
                // ============ CAMPOS ORIGINALES ============
                Date = repairDto.Date,
                Description = repairDto.Description,
                Cost = repairDto.Cost,
                WorkshopName = repairDto.WorkshopName,
                Mileage = repairDto.Mileage,
                IsWarranty = repairDto.IsWarranty,
                // ============ CAMPOS NUEVOS ============
                Type = repairDto.Type,
                Severity = repairDto.Severity,
                IsCoveredByInsurance = repairDto.IsCoveredByInsurance,
                InsurancePolicyId = repairDto.InsurancePolicyId,
                OccurredDuringRental = repairDto.OccurredDuringRental,
                RentalId = repairDto.RentalId,
                CustomerResponsible = repairDto.CustomerResponsible,
                CustomerCharge = repairDto.CustomerCharge,
                Status = repairDto.Status,
                Notes = repairDto.Notes
            };

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

        [HttpPut("repairs/{repairId}")]
        public async Task<IActionResult> UpdateRepair(int repairId, [FromBody] RepairForCreationDto repairDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var repairToUpdate = await _repairService.GetRepairByIdAsync(repairId);
            if (repairToUpdate == null) return NotFound();

            // Mapear todos los campos del DTO al modelo
            // ============ CAMPOS ORIGINALES ============
            repairToUpdate.Date = repairDto.Date;
            repairToUpdate.Description = repairDto.Description;
            repairToUpdate.Cost = repairDto.Cost;
            repairToUpdate.WorkshopName = repairDto.WorkshopName;
            repairToUpdate.Mileage = repairDto.Mileage;
            repairToUpdate.IsWarranty = repairDto.IsWarranty;
            // ============ CAMPOS NUEVOS ============
            repairToUpdate.Type = repairDto.Type;
            repairToUpdate.Severity = repairDto.Severity;
            repairToUpdate.IsCoveredByInsurance = repairDto.IsCoveredByInsurance;
            repairToUpdate.InsurancePolicyId = repairDto.InsurancePolicyId;
            repairToUpdate.OccurredDuringRental = repairDto.OccurredDuringRental;
            repairToUpdate.RentalId = repairDto.RentalId;
            repairToUpdate.CustomerResponsible = repairDto.CustomerResponsible;
            repairToUpdate.CustomerCharge = repairDto.CustomerCharge;
            repairToUpdate.Status = repairDto.Status;
            repairToUpdate.Notes = repairDto.Notes;

            await _repairService.UpdateRepairAsync(repairToUpdate);
            return NoContent();
        }

        [HttpDelete("repairs/{repairId}")]
        public async Task<IActionResult> DeleteRepair(int repairId)
        {
            await _repairService.DeleteRepairAsync(repairId);
            return NoContent();
        }
    }
}
