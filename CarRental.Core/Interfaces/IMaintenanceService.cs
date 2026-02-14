using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<Maintenance>> GetMaintenancesByVehicleIdAsync(int vehicleId);
        Task<Maintenance> GetMaintenanceByIdAsync(int id);
        Task<Maintenance> AddMaintenanceAsync(Maintenance maintenance);
        Task UpdateMaintenanceAsync(Maintenance maintenance);
        Task DeleteMaintenanceAsync(int id);
    }
}