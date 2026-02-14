// CarRental.Core/Models/Dtos/RentalForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.Rental; // Para usar los enums RentalStatus, RentalDestinationType, DriverLicenseCategory

namespace CarRental.Core.Models.Dtos
{
    public class RentalForCreationDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "La hora de entrega es obligatoria.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "El formato de hora debe ser HH:mm.")]
        public string StartTimeInput { get; set; } = "09:00"; // Campo de texto para la hora

        [Range(0.01, double.MaxValue, ErrorMessage = "La tarifa diaria debe ser mayor que cero.")]
        public decimal DailyRate { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres.")]
        public string Notes { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad de destino no puede exceder los 100 caracteres.")]
        public string DestinationCityName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de entrega es obligatorio.")]
        public int MileageAtDelivery { get; set; }

        public RentalDestinationType DestinationType { get; set; }

        [Required(ErrorMessage = "El nombre del conductor es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del conductor no puede exceder los 200 caracteres.")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "El tipo de licencia del conductor es obligatorio.")]
        public DriverLicenseCategory DriverLicenseType { get; set; }

        [Required(ErrorMessage = "La fecha de caducidad de la licencia del conductor es obligatoria.")]
        public DateTime DriverLicenseExpirationDate { get; set; }
    }
}
