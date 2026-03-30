using System;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.Maintenance;

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

        // ============ CAMPOS NUEVOS ============

        [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio.")]
        public MaintenanceType Type { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public MaintenanceCategory Category { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser positivo.")]
        public int? NextMaintenanceMileage { get; set; }

        [StringLength(50, ErrorMessage = "El número de factura no puede exceder 50 caracteres.")]
        public string? InvoiceNumber { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres.")]
        public string? Notes { get; set; }
    }
}