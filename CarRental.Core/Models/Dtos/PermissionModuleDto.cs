using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class PermissionModuleDto
    {
        public string Module { get; set; }
        public string ModuleName { get; set; }
        public int DisplayOrder { get; set; }
        public Permission ModulePermission { get; set; }
        public List<PermissionDto> Actions { get; set; } = new List<PermissionDto>();
    }
}
