namespace CarRental.Core.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public UserRole Role { get; set; }
        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
        public bool IsGranted { get; set; } = true;
    }
}