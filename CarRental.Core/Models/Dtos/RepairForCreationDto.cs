using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class RepairForCreationDto
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
        public int Mileage { get; set; }
        public bool IsWarranty { get; set; }
    }
}