using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class UserPermissionUpdate
    {
        public int PermissionId { get; set; }
        public bool IsGranted { get; set; }
    }
}
