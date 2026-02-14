using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    // DTO para representar un permiso individual
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public bool IsActive { get; set; }
        public PermissionType Type { get; set; }
        public int? ParentPermissionId { get; set; }
        public int DisplayOrder { get; set; }
    }

}