namespace CarRental.Core.Models.Dtos
{
    // Using CarRental.Core.Models because VehicleType enum is defined within the Vehicle class in that namespace.
    using CarRental.Core.Models;
    using static CarRental.Core.Models.Vehicle;

    public class VehicleFinancialReportDto
    {
        public int VehicleId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public VehicleType Type { get; set; } // Using the VehicleType enum
        public string LicensePlate { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsCompanyVehicle { get; set; }
    }
}
