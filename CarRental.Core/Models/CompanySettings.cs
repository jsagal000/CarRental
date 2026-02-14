namespace CarRental.Core.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone1 { get; set; } = string.Empty;
        public string Phone2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public byte[]? Logo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}