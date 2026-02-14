// CarRental.Core/Interfaces/IRentalRepository.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IRentalRepository : IGenericRepository<Rental>
    {
        Task<IEnumerable<Rental>> GetAllRentalsWithDetailsAsync();
        Task<Rental> GetRentalWithDetailsByIdAsync(int id);
        Task<IEnumerable<Rental>> GetUpcomingRentalsAsync(DateTime cutoffDate); // Para obtener rentas futuras/pendientes
    }
}
