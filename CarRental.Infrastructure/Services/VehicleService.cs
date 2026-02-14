// CarRental.Infrastructure/Services/VehicleService.cs
using CarRental.Core.Interfaces;
using CarRental.Infrastructure.Interfaces;
using CarRental.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllAsync();
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int id)
        {
            return await _vehicleRepository.GetByIdAsync(id);
        }

        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
        {
            // Aquí puedes añadir lógica de negocio adicional antes de guardar
            // Por ejemplo, validaciones más complejas
            await _vehicleRepository.AddAsync(vehicle);
            return vehicle; // Devuelve el vehículo con el ID generado si la base de datos lo asigna
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            // Lógica de negocio antes de actualizar
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            // Lógica de negocio antes de eliminar
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            // La lógica de disponibilidad ya está en el repositorio, el servicio solo la expone
            return await _vehicleRepository.GetAvailableVehiclesAsync(startDate, endDate);
        }
    }
}
