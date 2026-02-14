// CarRental.Core/Models/Rental.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Models
{
    public class Rental
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.DateTime)] // Propiedad DateTime para almacenar fecha y hora combinadas
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.DateTime)] // Propiedad DateTime para almacenar fecha y hora combinadas
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }


        //[Required(ErrorMessage = "La hora de entrega es obligatoria.")]
        //[RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "El formato de hora debe ser HH:mm.")]
        //public string StartTimeInput { get; set; } = "09:00"; // Campo de texto para la hora


        [Range(0.01, double.MaxValue, ErrorMessage = "La tarifa diaria debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")] // Define la precisión y escala para la base de datos
        public decimal DailyRate { get; set; } // Tarifa acordada para ESTE alquiler

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; } // Costo total calculado del alquiler

        public RentalStatus Status { get; set; } = RentalStatus.Reservado; // Estado por defecto en español

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres.")]
        public string Notes { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ActualReturnDate { get; set; } // Nulo, se establece al devolver

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OverdueCharges { get; set; } = 0.00m; // Cargos por retraso

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100, ErrorMessage = "La ciudad de destino no puede exceder los 100 caracteres.")]
        public string DestinationCityName { get; set; } // Nombre de la ciudad de destino

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de entrega es obligatorio.")]
        public int MileageAtDelivery { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de devolución debe ser un número positivo.")]
        public int? MileageAtReturn { get; set; } // Nulo, se llenará al finalizar la renta

        [Required(ErrorMessage = "El tipo de destino es obligatorio.")]
        public RentalDestinationType DestinationType { get; set; }

        // <<-- CAMPOS DEL CONDUCTOR -->>
        [Required(ErrorMessage = "El nombre del conductor es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del conductor no puede exceder los 200 caracteres.")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "El tipo de licencia del conductor es obligatorio.")]
        public DriverLicenseCategory DriverLicenseType { get; set; } // Referencia al enum anidado (Rental.DriverLicenseType)

        [Required(ErrorMessage = "La fecha de caducidad de la licencia del conductor es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DriverLicenseExpirationDate { get; set; }
        // <<-- FIN CAMPOS DEL CONDUCTOR -->>


        // Enum para el estado del alquiler (en español)
        public enum RentalStatus
        {
            Reservado,    // Reservada, pero aún no comenzó
            Activo,       // Actualmente alquilado
            Completado,   // Regresado a tiempo
            Vencido,      // Regresó tarde
            Cancelado,    // Reserva cancelada
            Dañado       // Devuelto con daños (puede requerir procesamiento adicional)
        }

        // Enum para el tipo de destino (en español y con solo dos opciones: Local y Nacional)
        public enum RentalDestinationType
        {
            Local,
            Nacional
        }

        // <<-- ESTA ES LA ÚNICA DEFINICIÓN VÁLIDA DEL ENUM DriverLicenseType DENTRO DE Rental -->>
        public enum DriverLicenseCategory
        {
            B, // Licencia de vehículos ligeros
            C, // Licencia de vehículos de transporte de pasajeros
            D, // Licencia de vehículos de transporte de carga
            E  // Licencia de vehículos especiales o pesados
        }
    }
}
