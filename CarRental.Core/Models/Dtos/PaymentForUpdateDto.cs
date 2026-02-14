// CarRental.Core/Models/Dtos/PaymentForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using static CarRental.Core.Models.Payment;

namespace CarRental.Core.Models.Dtos
{
    public class PaymentForUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El alquiler es obligatorio.")]
        public int RentalId { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public PaymentMethod Method { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres.")]
        public string Notes { get; set; }

        [StringLength(100, ErrorMessage = "El nombre del banco no puede exceder los 100 caracteres.")]
        public string BankName { get; set; }

        public BankAccountType? AccountType { get; set; }

        [StringLength(50, ErrorMessage = "El número de cuenta no puede exceder los 50 caracteres.")]
        public string AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "El número de referencia no puede exceder los 100 caracteres.")]
        public string ReferenceNumber { get; set; }
    }
}