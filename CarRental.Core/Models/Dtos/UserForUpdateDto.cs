using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class UserForUpdateDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public UserRole Role { get; set; }

        public bool IsActive { get; set; }
    }
}
