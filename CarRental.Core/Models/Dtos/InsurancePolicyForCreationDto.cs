using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    // Este es un Objeto de Transferencia de Datos (DTO).
    // Solo contiene los campos que el usuario rellena en el formulario.
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
    }
}