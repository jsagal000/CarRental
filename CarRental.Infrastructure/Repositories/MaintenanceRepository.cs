using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;

namespace CarRental.Infrastructure.Repositories
{
    public class MaintenanceRepository : GenericRepository<Maintenance>, IMaintenanceRepository
    {
        public MaintenanceRepository(CarRentalDbContext context) : base(context)
        {
        }
    }
}