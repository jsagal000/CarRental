using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Module { get; set; } // "Customer", "Vehicle", "Rental", "Partner"

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } // "Create", "Edit", "Delete", "View"

        public int? EntityId { get; set; } // ID del registro afectado

        [MaxLength(100)]
        public string? EntityName { get; set; } // Nombre o descripción del registro

        [MaxLength(500)]
        public string? Description { get; set; } // Descripción detallada de la acción

        public string? OldValues { get; set; } // JSON con valores anteriores (para Edit/Delete)
        public string? NewValues { get; set; } // JSON con valores nuevos (para Create/Edit)

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSuccess { get; set; } = true;

        [MaxLength(500)]
        public string? ErrorMessage { get; set; } // Si la acción falló
    }

    public enum AuditAction
    {
        Create,
        Edit,
        Delete,
        View,
        Login,
        Logout,
        PasswordChange,
        PermissionChange
    }

    public enum AuditModule
    {
        Customer,
        Vehicle,
        Rental,
        Partner,
        User,
        System
    }
}