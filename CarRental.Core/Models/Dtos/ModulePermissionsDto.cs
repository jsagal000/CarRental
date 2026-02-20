namespace CarRental.Core.Models.Dtos
{
    public class ModulePermissionsDto
    {
        public bool HasAccess { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
