using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class PermissionStatus
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public PermissionType Type { get; set; }
        public int? ParentPermissionId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsGranted { get; set; }
        public bool IsFromRole { get; set; }
        public bool IsDenied { get; set; }
    }
}
