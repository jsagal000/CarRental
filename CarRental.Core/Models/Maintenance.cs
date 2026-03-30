using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRental.Core.Models
{
    public class Maintenance
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        [JsonIgnore]
        public virtual Vehicle Vehicle { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "El costo es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cost { get; set; }

        [StringLength(150)]
        public string WorkshopName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje no puede ser negativo.")]
        public int Mileage { get; set; }

        // ============ CAMPOS NUEVOS - CRÍTICOS ============

        [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio.")]
        public MaintenanceType Type { get; set; } = MaintenanceType.Preventivo;

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public MaintenanceCategory Category { get; set; } = MaintenanceCategory.Revision_General;

        // Próximo mantenimiento programado
        public DateTime? NextMaintenanceDate { get; set; }

        // Kilometraje para próximo mantenimiento
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser positivo.")]
        public int? NextMaintenanceMileage { get; set; }

        // Número de factura o recibo
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        // Notas adicionales
        [StringLength(1000)]
        public string? Notes { get; set; }

        // ============ ENUMS ============

        public enum MaintenanceType
        {
            Preventivo,    // Mantenimiento programado para prevenir fallas
            Correctivo,    // Reparar algo que ya se dañó
            Predictivo,    // Basado en diagnósticos
            Programado     // Según manual del fabricante
        }

        public enum MaintenanceCategory
        {
            Motor,
            Transmision,
            Frenos,
            Suspension,
            Sistema_Electrico,
            Neumaticos,
            Aire_Acondicionado,
            Carroceria,
            Revision_General,
            Otros
        }
    }
}