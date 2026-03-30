using System;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.Repair;

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

        // ============ CAMPOS NUEVOS ============

        [Required(ErrorMessage = "El tipo de reparación es obligatorio.")]
        public RepairType Type { get; set; }

        [Required(ErrorMessage = "La severidad es obligatoria.")]
        public RepairSeverity Severity { get; set; }

        public bool IsCoveredByInsurance { get; set; } = false;

        public int? InsurancePolicyId { get; set; }

        public bool OccurredDuringRental { get; set; } = false;

        public int? RentalId { get; set; }

        public bool CustomerResponsible { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "El cargo debe ser positivo.")]
        public decimal? CustomerCharge { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public RepairStatus Status { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres.")]
        public string? Notes { get; set; }
    }
}