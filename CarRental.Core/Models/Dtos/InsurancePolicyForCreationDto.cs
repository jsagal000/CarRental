using System;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.InsurancePolicy;

namespace CarRental.Core.Models.Dtos
{

    public class InsurancePolicyForCreationDto
    {
        [Required(ErrorMessage = "El nombre de la aseguradora es obligatorio.")]
        [StringLength(100)]
        public string InsurerName { get; set; }

        [Required(ErrorMessage = "El tipo de seguro es obligatorio.")]
        [StringLength(100)]
        public string PolicyType { get; set; }

        [Required(ErrorMessage = "El monto de cobertura es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de cobertura debe ser mayor que cero.")]
        public decimal CoverageAmount { get; set; }

        [Required(ErrorMessage = "La tasa es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La tasa debe ser mayor que cero.")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "La prima es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La prima debe ser mayor que cero.")]
        public decimal InsurancePremium { get; set; }

        [Range(0, 36, ErrorMessage = "Las cuotas deben estar entre 0 y 36")]
        public int NumberOfInstallments { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "La cuota mensual debe ser mayor que cero.")]
        public decimal MonthlyInstallment {  get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }

        // ============ CAMPOS NUEVOS ============
        [Required(ErrorMessage = "El estado de la poliza de seguro es obligatorio.")]
        public PolicyStatus Status { get; set; }
        [Required(ErrorMessage = "El tipo de seguro es obligatorio.")]
        public InsurancePolicyType TypePolicy { get; set; }

        [Required(ErrorMessage = "El deducible es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El deducible debe ser positivo.")]
        public decimal Deducible { get; set; }

        public bool AutoRenew { get; set; } = false;

        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
        public string? EmergencyPhone { get; set; }

        [StringLength(100, ErrorMessage = "El nombre del agente no puede exceder 100 caracteres.")]
        public string? AgentName { get; set; }

        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
        public string? AgentPhone { get; set; }
    }
}