// CarRental.Core/Models/Payment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El alquiler es obligatorio.")]
        public int RentalId { get; set; }
        [ForeignKey("RentalId")]
        public Rental Rental { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public PaymentMethod Method { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres.")]
        public string Notes { get; set; }

        // Campos opcionales para información bancaria
        [StringLength(100, ErrorMessage = "El nombre del banco no puede exceder los 100 caracteres.")]
        public string BankName { get; set; }

        public BankAccountType? AccountType { get; set; }

        [StringLength(50, ErrorMessage = "El número de cuenta no puede exceder los 50 caracteres.")]
        public string AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "El número de referencia no puede exceder los 100 caracteres.")]
        public string ReferenceNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100, ErrorMessage = "El nombre del usuario no puede exceder los 100 caracteres.")]
        public string CreatedBy { get; set; }

        // Enum para método de pago
        public enum PaymentMethod
        {
            Efectivo,
            Transferencia,
            Deposito,
            Cheque,
            TarjetaCredito,
            TarjetaDebito
        }

        // Enum para tipo de cuenta bancaria
        public enum BankAccountType
        {
            Corriente,
            Ahorros
        }
    }
}