using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class InsurancePoliciesController : ControllerBase
    {
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly ILogger<InsurancePoliciesController> _logger;

        public InsurancePoliciesController(IInsurancePolicyService insurancePolicyService, ILogger<InsurancePoliciesController> logger)
        {
            _insurancePolicyService = insurancePolicyService;
            _logger = logger;
        }

        [HttpGet("vehicles/{vehicleId}/insurancepolicies")]
        public async Task<IActionResult> GetPoliciesByVehicle(int vehicleId)
        {
            var policies = await _insurancePolicyService.GetPoliciesByVehicleIdAsync(vehicleId);
            return Ok(policies);
        }

        // ... (resto de los métodos GET y DELETE sin cambios) ...

        [HttpPost("vehicles/{vehicleId}/insurancepolicies")]
        public async Task<IActionResult> CreatePolicy(int vehicleId, [FromBody] InsurancePolicyForCreationDto policyDto)
        {
            if (!ModelState.IsValid)
            {
                // Si el DTO no es válido, se retorna el error inmediatamente.
                _logger.LogWarning("API: DTO no válido al crear póliza. Errores: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            // Mapeamos manualmente el DTO al modelo de base de datos
            var policy = new InsurancePolicy
            {
                VehicleId = vehicleId, // <-- Tomado de la URL
                InsurerName = policyDto.InsurerName,
                PolicyType = policyDto.PolicyType,
                CoverageAmount = policyDto.CoverageAmount,
                Rate = policyDto.Rate,
                NumberOfInstallments = policyDto.NumberOfInstallments,
                StartDate = policyDto.StartDate,
                EndDate = policyDto.EndDate
            };

            var newPolicy = await _insurancePolicyService.AddPolicyAsync(policy);
            return CreatedAtAction(nameof(GetPolicyById), new { policyId = newPolicy.Id }, newPolicy);
        }

        [HttpPut("insurancepolicies/{policyId}")]
        public async Task<IActionResult> UpdatePolicy(int policyId, [FromBody] InsurancePolicyForCreationDto policyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var policyToUpdate = await _insurancePolicyService.GetPolicyByIdAsync(policyId);
            if (policyToUpdate == null)
            {
                return NotFound();
            }

            // Mapeamos los campos del DTO al modelo existente
            policyToUpdate.InsurerName = policyDto.InsurerName;
            policyToUpdate.PolicyType = policyDto.PolicyType;
            policyToUpdate.CoverageAmount = policyDto.CoverageAmount;
            policyToUpdate.Rate = policyDto.Rate;
            policyToUpdate.NumberOfInstallments = policyDto.NumberOfInstallments;
            policyToUpdate.StartDate = policyDto.StartDate;
            policyToUpdate.EndDate = policyDto.EndDate;

            await _insurancePolicyService.UpdatePolicyAsync(policyToUpdate);
            return NoContent();
        }

        // --- MÉTODOS SIN CAMBIOS ---
        [HttpGet("insurancepolicies/{policyId}", Name = "GetPolicyById")]
        public async Task<IActionResult> GetPolicyById(int policyId)
        {
            var policy = await _insurancePolicyService.GetPolicyByIdAsync(policyId);
            if (policy == null) return NotFound();
            return Ok(policy);
        }

        [HttpDelete("insurancepolicies/{policyId}")]
        public async Task<IActionResult> DeletePolicy(int policyId)
        {
            await _insurancePolicyService.DeletePolicyAsync(policyId);
            return NoContent();
        }
    }
}