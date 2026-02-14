// CarRental.Core/Interfaces/ICustomerRepository.cs
using CarRental.Core.Models;

namespace CarRental.Infrastructure.Interfaces
{
    // Heredamos de IGenericRepository para obtener las operaciones CRUD básicas
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        // Puedes añadir métodos específicos para clientes aquí si son necesarios.
        // Por ejemplo: Task<Customer> GetCustomerByLicenseNumberAsync(string licenseNumber);
    }
}
