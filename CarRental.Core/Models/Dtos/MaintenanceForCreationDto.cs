using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class MaintenanceForCreationDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "El costo es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor que cero.")]
        public decimal Cost { get; set; }

        [StringLength(150)]
        public string WorkshopName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje no puede ser negativo.")]
        public int Mileage { get; set; }
    }
}