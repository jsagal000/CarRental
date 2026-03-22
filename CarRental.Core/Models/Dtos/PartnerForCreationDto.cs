// CarRental.Core/Models/Dtos/PartnerForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class PartnerForCreationDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        public Partner.DocumentType TypeOfDocument { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de documento no puede exceder los 50 caracteres.")]
        public string DocumentNumber { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(150, ErrorMessage = "El correo electrónico no puede exceder los 150 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder los 20 caracteres.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "El país es obligatorio.")]
        [StringLength(100, ErrorMessage = "El país no puede exceder los 100 caracteres.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "La provincia es obligatoria.")]
        [StringLength(100, ErrorMessage = "La provincia no puede exceder los 100 caracteres.")]
        public string Province { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres.")]
        public string City { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "El banco es obligatorio.")]
        [StringLength(100, ErrorMessage = "El banco no puede exceder los 100 caracteres.")]
        public string Bank { get; set; }

        [Required(ErrorMessage = "El tipo de cuenta es obligatorio.")]
        public Partner.AccountType TypeOfAccount { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        [StringLength(30, ErrorMessage = "El número de cuenta no puede exceder los 30 caracteres.")]
        public string AccountNumber { get; set; }
    }
}