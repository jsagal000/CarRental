// CarRental.Core/Interfaces/IDashboardService.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<IEnumerable<Rental>> GetActiveRentalsAsync();
        Task<IEnumerable<Customer>> GetTodayBirthdaysAsync();
        Task<DashboardStatistics> GetDashboardStatisticsAsync();
        Task<IEnumerable<VehicleInfo>> GetVehiclesInfoAsync();
    }

    // Clase para las estadísticas del dashboard
    public class DashboardStatistics
    {
        public int TotalActiveRentals { get; set; }
        public int TotalReservedRentals { get; set; }
        public int TotalOverdueRentals { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalCustomers { get; set; }
    }

    // Clase para información de vehículos
    public class VehicleInfo
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal DailyRate { get; set; }
        public string State { get; set; }
        public string Ownership { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EndDate { get; set; }
    }
}