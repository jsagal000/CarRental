using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class UserPermissionDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public UserRole Role { get; set; }
        public List<PermissionStatus> Permissions { get; set; } = new List<PermissionStatus>();
    }
}
