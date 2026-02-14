// CarRental.Core/Interfaces/IVehicleRepository.cs
using CarRental.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        // Puedes añadir métodos específicos para vehículos aquí, por ejemplo:
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate);
    }
}
