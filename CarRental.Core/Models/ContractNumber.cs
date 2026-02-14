namespace CarRental.Core.Models
{
    public class ContractNumber
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public string ContractCode { get; set; } = string.Empty; // Ejemplo: "CONT-2022-0001"
        public int SequentialNumber { get; set; }
        public int Year { get; set; }
        public DateTime GeneratedDate { get; set; }

        // Relación
        public Rental? Rental { get; set; }
    }
}