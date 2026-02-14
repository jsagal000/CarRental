using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models
{
    // Enum para tipo de permiso
    public enum PermissionType
    {
        Module = 1,  // Permiso de acceso al módulo completo
        Action = 2   // Permiso de acción específica dentro del módulo
    }

    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // ej: "Customer.View", "Customer.Create", "Customer.Access"

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Module { get; set; } // ej: "Customer", "Vehicle", "Rental", "Partner"

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } // ej: "Access", "Create", "Edit", "Delete", "View"

        public PermissionType Type { get; set; } = PermissionType.Action; // Tipo de permiso

        public int? ParentPermissionId { get; set; } // Para jerarquía (acciones apuntan a módulo)
        public Permission ParentPermission { get; set; }

        public int DisplayOrder { get; set; } // Para ordenar en la UI

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }

    // Estructura de permisos por defecto para inicialización
    public static class DefaultPermissions
    {
        public static List<PermissionDefinition> GetDefaultPermissions()
        {
            return new List<PermissionDefinition>
            {
                // CLIENTES
                new PermissionDefinition
                {
                    Module = "Customer",
                    ModuleName = "Clientes",
                    DisplayOrder = 1,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver clientes", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Create", Name = "Crear clientes", DisplayOrder = 2 },
                        new ActionDefinition { Action = "Edit", Name = "Editar clientes", DisplayOrder = 3 },
                        new ActionDefinition { Action = "Delete", Name = "Eliminar clientes", DisplayOrder = 4 },
                        new ActionDefinition { Action = "Export", Name = "Exportar clientes", DisplayOrder = 5 }
                    }
                },

                // VEHÍCULOS
                new PermissionDefinition
                {
                    Module = "Vehicle",
                    ModuleName = "Vehículos",
                    DisplayOrder = 2,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver vehículos", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Create", Name = "Crear vehículos", DisplayOrder = 2 },
                        new ActionDefinition { Action = "Edit", Name = "Editar vehículos", DisplayOrder = 3 },
                        new ActionDefinition { Action = "Delete", Name = "Eliminar vehículos", DisplayOrder = 4 },
                        new ActionDefinition { Action = "Maintenance", Name = "Gestionar mantenimiento", DisplayOrder = 5 },
                        new ActionDefinition { Action = "Export", Name = "Exportar vehículos", DisplayOrder = 6 }
                    }
                },

                // ALQUILERES
                new PermissionDefinition
                {
                    Module = "Rental",
                    ModuleName = "Alquileres",
                    DisplayOrder = 3,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver alquileres", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Create", Name = "Crear alquileres", DisplayOrder = 2 },
                        new ActionDefinition { Action = "Edit", Name = "Editar alquileres", DisplayOrder = 3 },
                        new ActionDefinition { Action = "Delete", Name = "Eliminar alquileres", DisplayOrder = 4 },
                        new ActionDefinition { Action = "Cancel", Name = "Cancelar alquileres", DisplayOrder = 5 },
                        new ActionDefinition { Action = "GenerateContract", Name = "Generar contratos", DisplayOrder = 6 },
                        new ActionDefinition { Action = "Export", Name = "Exportar alquileres", DisplayOrder = 7 }
                    }
                },

                // SOCIOS
                new PermissionDefinition
                {
                    Module = "Partner",
                    ModuleName = "Socios",
                    DisplayOrder = 4,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver socios", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Create", Name = "Crear socios", DisplayOrder = 2 },
                        new ActionDefinition { Action = "Edit", Name = "Editar socios", DisplayOrder = 3 },
                        new ActionDefinition { Action = "Delete", Name = "Eliminar socios", DisplayOrder = 4 },
                        new ActionDefinition { Action = "ViewFinancials", Name = "Ver finanzas de socios", DisplayOrder = 5 }
                    }
                },

                // REPORTES FINANCIEROS
                new PermissionDefinition
                {
                    Module = "FinancialReports",
                    ModuleName = "Reportes Financieros",
                    DisplayOrder = 5,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver reportes", DisplayOrder = 1 },
                        new ActionDefinition { Action = "ExportPdf", Name = "Exportar a PDF", DisplayOrder = 2 },
                        new ActionDefinition { Action = "ExportExcel", Name = "Exportar a Excel", DisplayOrder = 3 },
                        new ActionDefinition { Action = "ViewDetailed", Name = "Ver reportes detallados", DisplayOrder = 4 }
                    }
                },

                // USUARIOS
                new PermissionDefinition
                {
                    Module = "User",
                    ModuleName = "Usuarios",
                    DisplayOrder = 6,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver usuarios", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Create", Name = "Crear usuarios", DisplayOrder = 2 },
                        new ActionDefinition { Action = "Edit", Name = "Editar usuarios", DisplayOrder = 3 },
                        new ActionDefinition { Action = "Delete", Name = "Eliminar usuarios", DisplayOrder = 4 },
                        new ActionDefinition { Action = "ResetPassword", Name = "Resetear contraseñas", DisplayOrder = 5 }
                    }
                },

                // PERMISOS
                new PermissionDefinition
                {
                    Module = "Permission",
                    ModuleName = "Permisos",
                    DisplayOrder = 7,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver permisos", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Manage", Name = "Gestionar permisos", DisplayOrder = 2 }
                    }
                },

                // AUDITORÍA
                new PermissionDefinition
                {
                    Module = "Audit",
                    ModuleName = "Auditoría",
                    DisplayOrder = 8,
                    Actions = new List<ActionDefinition>
                    {
                        new ActionDefinition { Action = "View", Name = "Ver auditoría", DisplayOrder = 1 },
                        new ActionDefinition { Action = "Export", Name = "Exportar auditoría", DisplayOrder = 2 }
                    }
                }
            };
        }
    }

    public class PermissionDefinition
    {
        public string Module { get; set; }
        public string ModuleName { get; set; }
        public int DisplayOrder { get; set; }
        public List<ActionDefinition> Actions { get; set; } = new List<ActionDefinition>();
    }

    public class ActionDefinition
    {
        public string Action { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}