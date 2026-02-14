using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IRepairService
    {
        Task<IEnumerable<Repair>> GetRepairsByVehicleIdAsync(int vehicleId);
        Task<Repair> GetRepairByIdAsync(int id);
        Task<Repair> AddRepairAsync(Repair repair);
        Task UpdateRepairAsync(Repair repair);
        Task DeleteRepairAsync(int id);
    }
}