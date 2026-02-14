using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
namespace CarRental.Infrastructure.Repositories
{
    public class InsurancePolicyRepository : GenericRepository<InsurancePolicy>, IInsurancePolicyRepository
    {
        public InsurancePolicyRepository(CarRentalDbContext context) : base(context) { }
    }
}