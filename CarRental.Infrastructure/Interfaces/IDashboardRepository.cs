// CarRental.Infrastructure/Interfaces/IDashboardRepository.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<Rental>> GetActiveRentalsAsync();
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<int> CountRentalsByStatusAsync(Rental.RentalStatus status);
        Task<int> CountVehiclesByStateAsync(Vehicle.VehicleState state);
        Task<int> CountAllCustomersAsync();
        Task<IEnumerable<Vehicle>> GetVehiclesWithRentalsAsync();
        Task<Rental> GetActiveRentalForVehicle(int vehicleId);
    }
}