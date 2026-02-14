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
    public class CustomersController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly IAuditService _auditService;

        public CustomersController(CarRentalDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        [RequireCustomerView]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            var userId = GetCurrentUserId();

            try
            {
                var customers = await _context.Customers.ToListAsync();

                // NOTA: Ya no se auditan las consultas (View) según las mejores prácticas
                // Este código está comentado pero lo dejo por si necesitas habilitarlo temporalmente
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "View",
                    description: $"Consultó lista de clientes ({customers.Count} registros)",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return customers;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "View",
                    description: "Error al consultar lista de clientes",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [RequireCustomerView]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Customer",
                        action: "View",
                        entityId: id,
                        description: $"Intentó consultar cliente inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Cliente no encontrado"
                    );

                    return NotFound();
                }

                // NOTA: Consulta exitosa - No se audita según mejores prácticas
                /*
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "View",
                    entityId: customer.Id,
                    entityName: $"{customer.FirstName} {customer.LastName}",
                    description: $"Consultó detalles del cliente {customer.FirstName} {customer.LastName}",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );
                */

                return customer;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "View",
                    entityId: id,
                    description: $"Error al consultar cliente (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [RequireCustomerCreate]
        public async Task<ActionResult<Customer>> Create(CustomerForCreationDto dto)
        {
            var userId = GetCurrentUserId();

            try
            {
                var customer = new Customer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    TypeOfDocument = dto.TypeOfDocument,
                    DocumentNumber = dto.DocumentNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    City = dto.City,
                    StateProvince = dto.StateProvince,
                    Country = dto.Country,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Creación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "Create",
                    entityId: customer.Id,
                    entityName: $"{customer.FirstName} {customer.LastName}",
                    description: $"Creó nuevo cliente: {customer.FirstName} {customer.LastName} (Doc: {customer.DocumentNumber})",
                    newValues: new
                    {
                        customer.Id,
                        customer.FirstName,
                        customer.LastName,
                        customer.Email,
                        customer.PhoneNumber,
                        customer.TypeOfDocument,
                        customer.DocumentNumber,
                        customer.DateOfBirth,
                        customer.Address,
                        customer.City,
                        customer.StateProvince,
                        customer.Country
                    },
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "Create",
                    description: $"Error al crear cliente: {dto.FirstName} {dto.LastName}",
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
        [RequireCustomerEdit]
        public async Task<IActionResult> Update(int id, Customer customer)
        {
            var userId = GetCurrentUserId();

            if (id != customer.Id)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
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
                var existing = await _context.Customers.FindAsync(id);
                if (existing == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Customer",
                        action: "Edit",
                        entityId: id,
                        description: $"Intentó editar cliente inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Cliente no encontrado"
                    );

                    return NotFound();
                }

                // Capturar valores antiguos para auditoría
                var oldValues = new
                {
                    existing.FirstName,
                    existing.LastName,
                    existing.Email,
                    existing.PhoneNumber,
                    existing.TypeOfDocument,
                    existing.DocumentNumber,
                    existing.DateOfBirth,
                    existing.Address,
                    existing.City,
                    existing.StateProvince,
                    existing.Country
                };

                // Actualizar campos
                existing.FirstName = customer.FirstName;
                existing.LastName = customer.LastName;
                existing.Email = customer.Email;
                existing.PhoneNumber = customer.PhoneNumber;
                existing.TypeOfDocument = customer.TypeOfDocument;
                existing.DocumentNumber = customer.DocumentNumber;
                existing.DateOfBirth = customer.DateOfBirth;
                existing.Address = customer.Address;
                existing.City = customer.City;
                existing.StateProvince = customer.StateProvince;
                existing.Country = customer.Country;

                await _context.SaveChangesAsync();

                var newValues = new
                {
                    existing.FirstName,
                    existing.LastName,
                    existing.Email,
                    existing.PhoneNumber,
                    existing.TypeOfDocument,
                    existing.DocumentNumber,
                    existing.DateOfBirth,
                    existing.Address,
                    existing.City,
                    existing.StateProvince,
                    existing.Country
                };

                // ✅ AUDITAR: Edición de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "Edit",
                    entityId: existing.Id,
                    entityName: $"{existing.FirstName} {existing.LastName}",
                    description: $"Editó cliente: {existing.FirstName} {existing.LastName}",
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
                    module: "Customer",
                    action: "Edit",
                    entityId: id,
                    description: $"Error al editar cliente (ID: {id})",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [RequireCustomerDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    await _auditService.LogActionAsync(
                        userId: userId,
                        module: "Customer",
                        action: "Delete",
                        entityId: id,
                        description: $"Intentó eliminar cliente inexistente (ID: {id})",
                        ipAddress: HttpContext.GetClientIpAddress(),
                        userAgent: HttpContext.GetUserAgent(),
                        isSuccess: false,
                        errorMessage: "Cliente no encontrado"
                    );

                    return NotFound();
                }

                // Capturar datos antes de eliminar
                var deletedCustomerData = new
                {
                    customer.Id,
                    customer.FirstName,
                    customer.LastName,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.TypeOfDocument,
                    customer.DocumentNumber,
                    customer.DateOfBirth,
                    customer.Address,
                    customer.City,
                    customer.StateProvince,
                    customer.Country,
                    customer.RegistrationDate
                };

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                // ✅ AUDITAR: Eliminación de registro
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "Delete",
                    entityId: customer.Id,
                    entityName: $"{customer.FirstName} {customer.LastName}",
                    description: $"Eliminó cliente: {customer.FirstName} {customer.LastName} (Doc: {customer.DocumentNumber})",
                    oldValues: deletedCustomerData,
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent()
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Customer",
                    action: "Delete",
                    entityId: id,
                    description: $"Error al eliminar cliente (ID: {id})",
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