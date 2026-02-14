using CarRental.Api.Extensions;
using CarRental.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CarRental.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string Module { get; }
        public string Action { get; }

        public RequirePermissionAttribute(string module, string action)
        {
            Module = module;
            Action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var permissionResult = await permissionService.HasPermissionAsync(userId, Module, Action);

            // Obtener IP y User Agent correctamente usando los helpers
            var ipAddress = context.HttpContext.GetClientIpAddress();
            var userAgent = context.HttpContext.GetUserAgent();

            if (!permissionResult.IsSuccess || !permissionResult.Data)
            {
                // Intentar obtener el servicio de auditoría para registrar acceso denegado
                var auditService = context.HttpContext.RequestServices.GetService<IAuditService>();

                if (auditService != null)
                {
                    await auditService.LogActionAsync(
                        userId: userId,
                        module: Module,
                        action: Action,
                        description: $"Intento de acceso denegado a {Module}.{Action}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        isSuccess: false,
                        errorMessage: "Acceso denegado - Permisos insuficientes"
                    );
                }

                context.Result = new ForbidResult();
                return;
            }

            // OPCIONAL: Descomentar si quieres auditar también los accesos exitosos
            // (No recomendado porque generaría muchos logs)
            /*
            var auditService = context.HttpContext.RequestServices.GetService<IAuditService>();
            if (auditService != null)
            {
                await auditService.LogActionAsync(
                    userId: userId,
                    module: Module,
                    action: Action,
                    description: $"Acceso autorizado a {Module}.{Action}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    isSuccess: true
                );
            }
            */
        }
    }

    // Helper attributes para simplificar el uso
    public class RequireCustomerViewAttribute : RequirePermissionAttribute
    {
        public RequireCustomerViewAttribute() : base("Customer", "View") { }
    }

    public class RequireCustomerCreateAttribute : RequirePermissionAttribute
    {
        public RequireCustomerCreateAttribute() : base("Customer", "Create") { }
    }

    public class RequireCustomerEditAttribute : RequirePermissionAttribute
    {
        public RequireCustomerEditAttribute() : base("Customer", "Edit") { }
    }

    public class RequireCustomerDeleteAttribute : RequirePermissionAttribute
    {
        public RequireCustomerDeleteAttribute() : base("Customer", "Delete") { }
    }

    // Helper attributes para simplificar el uso con Vehicle
    public class RequireVehicleViewAttribute : RequirePermissionAttribute
    {
        public RequireVehicleViewAttribute() : base("Vehicle", "View") { }
    }

    public class RequireVehicleCreateAttribute : RequirePermissionAttribute
    {
        public RequireVehicleCreateAttribute() : base("Vehicle", "Create") { }
    }

    public class RequireVehicleEditAttribute : RequirePermissionAttribute
    {
        public RequireVehicleEditAttribute() : base("Vehicle", "Edit") { }
    }

    public class RequireVehicleDeleteAttribute : RequirePermissionAttribute
    {
        public RequireVehicleDeleteAttribute() : base("Vehicle", "Delete") { }
    }

    // Helper attributes para Partner
    public class RequirePartnerViewAttribute : RequirePermissionAttribute
    {
        public RequirePartnerViewAttribute() : base("Partner", "View") { }
    }

    public class RequirePartnerCreateAttribute : RequirePermissionAttribute
    {
        public RequirePartnerCreateAttribute() : base("Partner", "Create") { }
    }

    public class RequirePartnerEditAttribute : RequirePermissionAttribute
    {
        public RequirePartnerEditAttribute() : base("Partner", "Edit") { }
    }

    public class RequirePartnerDeleteAttribute : RequirePermissionAttribute
    {
        public RequirePartnerDeleteAttribute() : base("Partner", "Delete") { }
    }

    // Helper attributes para Rental
    public class RequireRentalViewAttribute : RequirePermissionAttribute
    {
        public RequireRentalViewAttribute() : base("Rental", "View") { }
    }

    public class RequireRentalCreateAttribute : RequirePermissionAttribute
    {
        public RequireRentalCreateAttribute() : base("Rental", "Create") { }
    }

    public class RequireRentalEditAttribute : RequirePermissionAttribute
    {
        public RequireRentalEditAttribute() : base("Rental", "Edit") { }
    }

    public class RequireRentalDeleteAttribute : RequirePermissionAttribute
    {
        public RequireRentalDeleteAttribute() : base("Rental", "Delete") { }
    }
}