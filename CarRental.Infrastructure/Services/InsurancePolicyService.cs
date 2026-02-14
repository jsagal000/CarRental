using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class InsurancePolicyService : IInsurancePolicyService
    {
        private readonly CarRentalDbContext _context;

        public InsurancePolicyService(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<InsurancePolicy> AddPolicyAsync(InsurancePolicy policy)
        {
            _context.InsurancePolicies.Add(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<InsurancePolicy> GetPolicyByIdAsync(int id)
        {
            return await _context.InsurancePolicies.FindAsync(id);
        }

        public async Task<IEnumerable<InsurancePolicy>> GetPoliciesByVehicleIdAsync(int vehicleId)
        {
            return await _context.InsurancePolicies
                .Where(p => p.VehicleId == vehicleId)
                .ToListAsync();
        }

        public async Task UpdatePolicyAsync(InsurancePolicy policy)
        {
            _context.Entry(policy).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePolicyAsync(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy != null)
            {
                _context.InsurancePolicies.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }
    }
}