// CarRental.Infrastructure/Services/DashboardService.cs
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<IEnumerable<Rental>> GetActiveRentalsAsync()
        {
            return await _dashboardRepository.GetActiveRentalsAsync();
        }

        public async Task<IEnumerable<Customer>> GetTodayBirthdaysAsync()
        {
            var today = DateTime.Today;
            var allCustomers = await _dashboardRepository.GetAllCustomersAsync();

            // Filtrar clientes con cumpleaños hoy
            var birthdayCustomers = allCustomers
                .Where(c => c.DateOfBirth.HasValue &&
                           c.DateOfBirth.Value.Month == today.Month &&
                           c.DateOfBirth.Value.Day == today.Day)
                .OrderBy(c => c.FirstName)
                .ToList();

            return birthdayCustomers;
        }

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            var statistics = new DashboardStatistics
            {
                TotalActiveRentals = await _dashboardRepository.CountRentalsByStatusAsync(Rental.RentalStatus.Activo),
                TotalReservedRentals = await _dashboardRepository.CountRentalsByStatusAsync(Rental.RentalStatus.Reservado),
                TotalOverdueRentals = await _dashboardRepository.CountRentalsByStatusAsync(Rental.RentalStatus.Vencido),
                AvailableVehicles = await _dashboardRepository.CountVehiclesByStateAsync(Vehicle.VehicleState.Disponible),
                TotalCustomers = await _dashboardRepository.CountAllCustomersAsync()
            };

            return statistics;
        }

        public async Task<IEnumerable<VehicleInfo>> GetVehiclesInfoAsync()
        {
            var vehicles = await _dashboardRepository.GetVehiclesWithRentalsAsync();

            var vehicleInfoList = vehicles.Select(v => new VehicleInfo
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate,
                DailyRate = v.DailyRate,
                State = v.State.ToString(),
                Ownership = v.Ownership.HasValue ? v.Ownership.Value.ToString() : "Empresa",
                CustomerName = GetActiveRentalCustomerName(v),
                EndDate = GetActiveRentalEndDate(v)
            }).ToList();

            return vehicleInfoList;
        }

        private string GetActiveRentalCustomerName(Vehicle vehicle)
        {
            // Buscar el rental activo para este vehículo
            var activeRental = _dashboardRepository.GetActiveRentalForVehicle(vehicle.Id).Result;
            if (activeRental != null && activeRental.Customer != null)
            {
                return $"{activeRental.Customer.FirstName} {activeRental.Customer.LastName}";
            }
            return null;
        }

        private DateTime? GetActiveRentalEndDate(Vehicle vehicle)
        {
            // Buscar el rental activo para este vehículo
            var activeRental = _dashboardRepository.GetActiveRentalForVehicle(vehicle.Id).Result;
            return activeRental?.EndDate;
        }
    }
}