using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRental.Core.Models
{
    public class Repair
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
        public bool IsWarranty { get; set; }

        // ============ CAMPOS NUEVOS - CRÍTICOS ============

        [Required(ErrorMessage = "El tipo de reparación es obligatorio.")]
        public RepairType Type { get; set; } = RepairType.Fallo_Mecanico;

        [Required(ErrorMessage = "La severidad es obligatoria.")]
        public RepairSeverity Severity { get; set; } = RepairSeverity.Moderada;

        // Relación con Seguros
        public bool IsCoveredByInsurance { get; set; } = false;

        public int? InsurancePolicyId { get; set; }

        [ForeignKey("InsurancePolicyId")]
        [JsonIgnore]
        public virtual InsurancePolicy? InsurancePolicy { get; set; }

        // Relación con Alquileres
        public bool OccurredDuringRental { get; set; } = false;

        public int? RentalId { get; set; }

        [ForeignKey("RentalId")]
        [JsonIgnore]
        public virtual Rental? Rental { get; set; }

        // Responsabilidad del cliente
        public bool CustomerResponsible { get; set; } = false;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CustomerCharge { get; set; }

        // Estado de la reparación
        public RepairStatus Status { get; set; } = RepairStatus.Completada;

        // Notas adicionales
        [StringLength(1000)]
        public string? Notes { get; set; }

        // ============ ENUMS ============

        public enum RepairType
        {
            Accidente,        // Choque, colisión
            Desgaste,         // Uso normal
            Vandalismo,       // Daño intencional
            Fallo_Mecanico,   // Falla de componente
            Climatico,        // Granizo, inundación
            Otros
        }

        public enum RepairSeverity
        {
            Menor,      // Rayones, abolladuras pequeñas
            Moderada,   // Requiere algunas partes
            Grave,      // Daño estructural
            Total       // Pérdida total
        }

        public enum RepairStatus
        {
            Pendiente,
            En_Diagnostico,
            Aprobada,
            En_Proceso,
            En_Espera_Partes,
            Completada,
            Cancelada
        }
    }
}