// CarRental.Core/Models/Customer.cs
using System.ComponentModel.DataAnnotations; // For data annotations like [Required], [StringLength]

namespace CarRental.Core.Models
{
    public class Customer
    {
        public int Id { get; set; } // Unique identifier for the customer

        [Required(ErrorMessage = "El nombre es obligatorio.")] // Name is required
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")] // Max length for name
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")] // Last name is required
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")] // Max length for last name
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")] // Email format validation
        [StringLength(150, ErrorMessage = "El correo electrónico no puede exceder los 150 caracteres.")] // Max length for email
        public string? Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")] // Phone number is required
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")] // Phone format validation
        [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder los 20 caracteres.")] // Max length for phone
        public string PhoneNumber { get; set; }

        // Propiedades eliminadas: LicenseNumber, PostalCode

        // <<-- NUEVAS PROPIEDADES AÑADIDAS -->>
        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        public DocumentType TypeOfDocument { get; set; } // Nuevo campo para tipo de documento

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de documento no puede exceder los 50 caracteres.")]
        public string DocumentNumber { get; set; } // Nuevo campo para número de documento
        // <<-- FIN DE NUEVAS PROPIEDADES -->>

        // Optional: Date of birth
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; } // Nullable if not always required

        // Optional: Address fields
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
        public string Address { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres.")]
        public string City { get; set; }

        [StringLength(100, ErrorMessage = "El estado/provincia no puede exceder los 100 caracteres.")]
        public string StateProvince { get; set; }

        // Propiedad eliminada: PostalCode

        [StringLength(100, ErrorMessage = "El país no puede exceder los 100 caracteres.")]
        public string Country { get; set; }

        [StringLength(100, ErrorMessage = "La nacionalidad no puede exceder los 100 caracteres.")]
        public string Nationality { get; set; }

        // Optional: Registration Date
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow; // Automatically set to current UTC time

        // Enum para el tipo de documento (¡Con acento en Cédula!)
        public enum DocumentType
        {
            Cédula,
            RUC,
            Pasaporte
        }

    }
}
