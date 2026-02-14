using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IInsurancePolicyService
    {
        Task<IEnumerable<InsurancePolicy>> GetPoliciesByVehicleIdAsync(int vehicleId);
        Task<InsurancePolicy> GetPolicyByIdAsync(int id);
        Task<InsurancePolicy> AddPolicyAsync(InsurancePolicy policy);
        Task UpdatePolicyAsync(InsurancePolicy policy);
        Task DeletePolicyAsync(int id);
    }
}