using CarRental.Api.Attributes;
using CarRental.Api.Extensions;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PartnersController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly IAuditService _auditService;

        public PartnersController(CarRentalDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        [RequirePermission("Partner", "View")]
        public async Task<ActionResult<IEnumerable<Partner>>> GetAll()
        {
            var userId = GetCurrentUserId();

            try
            {
                var partners = await _context.Partners.ToListAsync();

                // NOTA: Ya no se auditan las consultas (View) según las mejores prácticas
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "View",
                    description: $"Consultó lista de socios ({partners.Count} registros)",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return partners;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "View",
                    description: "Error al consultar lista de socios",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("Partner", "View")]
        public async Task<ActionResult<Partner>> GetById(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Partner",
                        action: "View",
                        entityId: id,
                        description: $"Intentó consultar socio inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Socio no encontrado"
                    );

                    return NotFound();
                }

                // NOTA: Consulta exitosa - No se audita según mejores prácticas
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "View",
                    entityId: partner.Id,
                    entityName: $"{partner.FirstName} {partner.LastName}",
                    description: $"Consultó detalles del socio {partner.FirstName} {partner.LastName}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return partner;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "View",
                    entityId: id,
                    description: $"Error al consultar socio (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [RequirePermission("Partner", "Create")]
        public async Task<ActionResult<Partner>> Create(PartnerForCreationDto dto)
        {
            var userId = GetCurrentUserId();

            try
            {
                var partner = new Partner
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Cedula = dto.Cedula,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Province = dto.Province,
                    City = dto.City,
                    Address = dto.Address,
                    Bank = dto.Bank,
                    TypeOfAccount = dto.TypeOfAccount,
                    AccountNumber = dto.AccountNumber,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.Partners.Add(partner);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Creación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Create",
                    entityId: partner.Id,
                    entityName: $"{partner.FirstName} {partner.LastName}",
                    description: $"Creó nuevo socio: {partner.FirstName} {partner.LastName} (Cédula: {partner.Cedula})",
                    newValues: new
                    {
                        partner.Id,
                        partner.FirstName,
                        partner.LastName,
                        partner.Cedula,
                        partner.Email,
                        partner.PhoneNumber,
                        partner.Country,
                        partner.Province,
                        partner.City,
                        partner.Address,
                        partner.Bank,
                        partner.TypeOfAccount,
                        partner.AccountNumber
                    },
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return CreatedAtAction(nameof(GetById), new { id = partner.Id }, partner);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Create",
                    description: $"Error al crear socio: {dto.FirstName} {dto.LastName}",
                    newValues: dto,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Partner", "Edit")]
        public async Task<IActionResult> Update(int id, Partner partner)
        {
            var userId = GetCurrentUserId();

            if (id != partner.Id)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Edit",
                    entityId: id,
                    description: "Error de validación: ID no coincide",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: "ID no coincide"
                );

                return BadRequest();
            }

            try
            {
                var existing = await _context.Partners.FindAsync(id);
                if (existing == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Partner",
                        action: "Edit",
                        entityId: id,
                        description: $"Intentó editar socio inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Socio no encontrado"
                    );

                    return NotFound();
                }

                // Capturar valores antiguos para auditoría
                var oldValues = new
                {
                    existing.FirstName,
                    existing.LastName,
                    existing.Cedula,
                    existing.Email,
                    existing.PhoneNumber,
                    existing.Country,
                    existing.Province,
                    existing.City,
                    existing.Address,
                    existing.Bank,
                    existing.TypeOfAccount,
                    existing.AccountNumber
                };

                // Actualizar campos
                existing.FirstName = partner.FirstName;
                existing.LastName = partner.LastName;
                existing.Cedula = partner.Cedula;
                existing.Email = partner.Email;
                existing.PhoneNumber = partner.PhoneNumber;
                existing.Country = partner.Country;
                existing.Province = partner.Province;
                existing.City = partner.City;
                existing.Address = partner.Address;
                existing.Bank = partner.Bank;
                existing.TypeOfAccount = partner.TypeOfAccount;
                existing.AccountNumber = partner.AccountNumber;

                await _context.SaveChangesAsync();

                var newValues = new
                {
                    existing.FirstName,
                    existing.LastName,
                    existing.Cedula,
                    existing.Email,
                    existing.PhoneNumber,
                    existing.Country,
                    existing.Province,
                    existing.City,
                    existing.Address,
                    existing.Bank,
                    existing.TypeOfAccount,
                    existing.AccountNumber
                };

                // ✅ AUDITAR: Edición de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Edit",
                    entityId: existing.Id,
                    entityName: $"{existing.FirstName} {existing.LastName}",
                    description: $"Editó socio: {existing.FirstName} {existing.LastName}",
                    oldValues: oldValues,
                    newValues: newValues,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Edit",
                    entityId: id,
                    description: $"Error al editar socio (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Partner", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Partner",
                        action: "Delete",
                        entityId: id,
                        description: $"Intentó eliminar socio inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Socio no encontrado"
                    );

                    return NotFound();
                }

                // Capturar datos antes de eliminar
                var deletedPartnerData = new
                {
                    partner.Id,
                    partner.FirstName,
                    partner.LastName,
                    partner.Cedula,
                    partner.Email,
                    partner.PhoneNumber,
                    partner.Country,
                    partner.Province,
                    partner.City,
                    partner.Address,
                    partner.Bank,
                    partner.TypeOfAccount,
                    partner.AccountNumber,
                    partner.RegistrationDate
                };

                _context.Partners.Remove(partner);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Eliminación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Delete",
                    entityId: partner.Id,
                    entityName: $"{partner.FirstName} {partner.LastName}",
                    description: $"Eliminó socio: {partner.FirstName} {partner.LastName} (Cédula: {partner.Cedula})",
                    oldValues: deletedPartnerData,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Partner",
                    action: "Delete",
                    entityId: id,
                    description: $"Error al eliminar socio (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }
}