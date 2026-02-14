using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class UpdateUserPermissionsDto
    {
        public int UserId { get; set; }
        public List<UserPermissionUpdate> Permissions { get; set; } = new List<UserPermissionUpdate>();
    }
}
