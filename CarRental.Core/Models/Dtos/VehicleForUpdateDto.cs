using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.Vehicle;

namespace CarRental.Core.Models.Dtos
{
    public class VehicleForUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres.")]
        public string Make { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres.")]
        public string Model { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio.")]
        public VehicleType Type { get; set; }

        [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La matrícula no puede exceder los 20 caracteres.")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "La tarifa diaria es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La tarifa diaria debe ser mayor que cero.")]
        public decimal DailyRate { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public VehicleState State { get; set; }

        [Required(ErrorMessage = "El VIN es obligatorio.")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "El VIN debe tener 17 caracteres.")]
        public string Vin { get; set; }

        [Required(ErrorMessage = "El color es obligatorio.")]
        [StringLength(50, ErrorMessage = "El color no puede exceder los 50 caracteres.")]
        public string Color { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        // Campos para el propietario del vehículo
        public OwnershipType? Ownership { get; set; }
        public int? PartnerId { get; set; }
    }
}