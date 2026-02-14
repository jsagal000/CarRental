// CarRental.Core/Models/Dtos/MonthlyFinancialReportDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{

    public class MonthlyFinancialReportDto
    {

        [Required]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
        public int Month { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
        public int Year { get; set; }

        public string MonthName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El ingreso total no puede ser negativo.")]
        public decimal TotalRevenue { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El número de alquileres no puede ser negativo.")]
        public int NumberOfRentals { get; set; }
        public int RentalCount { get; set; }
    }
}
