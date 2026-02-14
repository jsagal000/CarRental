using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;

namespace CarRental.Infrastructure.Repositories
{
    public class RepairRepository : GenericRepository<Repair>, IRepairRepository
    {
        public RepairRepository(CarRentalDbContext context) : base(context)
        {
        }
    }
}