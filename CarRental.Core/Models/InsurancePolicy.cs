using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRental.Core.Models
{
    public class InsurancePolicy
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; } // Clave foránea

        [ForeignKey("VehicleId")]
        [JsonIgnore]
        public virtual Vehicle Vehicle { get; set; } // Propiedad de navegación

        [Required(ErrorMessage = "El nombre de la aseguradora es obligatorio.")]
        [StringLength(100)]
        public string InsurerName { get; set; }


        [Required(ErrorMessage = "El monto de cobertura es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de cobertura debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CoverageAmount { get; set; }

        [Required(ErrorMessage = "La tasa es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La tasa debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "La prima es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La prima debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal InsurancePremium { get; set; }

        [Range(0, 36, ErrorMessage = "Las cuotas deben estar entre 0 y 36")]
        public int NumberOfInstallments { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "La cuota mensual debe ser mayor que cero.")]
        public decimal MonthlyInstallment { get; set; }


        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddYears(1);

        // ============ CAMPOS NUEVOS - CRÍTICOS ============
        [Required(ErrorMessage = "El estado de la poliza de seguro es obligatorio.")]
        public PolicyStatus Status { get; set; } = PolicyStatus.Activa;

        [Required(ErrorMessage = "El tipo de seguro es obligatorio.")]
        public InsurancePolicyType TypePolicy { get; set; }

        [Required(ErrorMessage = "El deducible es obligatorio.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "El deducible debe ser positivo.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Deducible { get; set; }

        // ¿Se renueva automáticamente?
        public bool AutoRenew { get; set; } = false;

        // Teléfono de emergencia
        [Phone]
        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        // Agente de seguros
        [StringLength(100)]
        public string? AgentName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? AgentPhone { get; set; }

        // Relación con reparaciones
        [JsonIgnore]
        public virtual ICollection<Repair>? Repairs { get; set; }

        // ============ ENUMS ============

        public enum PolicyStatus
        {
            Activa,
            Vencida,
            Cancelada,
            Suspendida,
            Renovacion_Pendiente
        }

        public enum InsurancePolicyType
        {
            Todo_Riesgo,
            Responsabilidad_Civil,
            Daños_Propios,
            Robo_Incendio,
            Basico
        }
    }
}