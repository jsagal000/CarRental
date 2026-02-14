// CarRental.Core/Models/Expense.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; } // Clave foránea para el vehículo

        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; } // Propiedad de navegación

        [Required(ErrorMessage = "La categoría del gasto es obligatoria.")]
        public ExpenseCategory Category { get; set; }

        [Required(ErrorMessage = "El monto del gasto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser positivo.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La fecha del gasto es obligatoria.")]
        public DateTime ExpenseDate { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; }

        public int? Mileage { get; set; } // Kilometraje al momento del gasto (nulable)

        [StringLength(150)]
        public string WorkshopName { get; set; } // Nombre del taller (nulable)
    }
}