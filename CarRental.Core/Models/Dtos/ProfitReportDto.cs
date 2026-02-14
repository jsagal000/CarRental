namespace CarRental.Core.Models.Dtos
{
    public class ProfitReportDto
    {
        public int VehicleId { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal NetProfit { get; set; }
    }
}
