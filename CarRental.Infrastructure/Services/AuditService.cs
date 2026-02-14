using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Interfaces;
using System.Text.Json;

namespace CarRental.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task LogActionAsync(int userId, string module, string action, int? entityId = null,
            string entityName = null, string description = null, object oldValues = null,
            object newValues = null, string ipAddress = null, string userAgent = null,
            bool isSuccess = true, string errorMessage = null)
        {
            try
            {
                Console.WriteLine("=================================================");
                Console.WriteLine($"[AUDIT DEBUG] INICIO - LogActionAsync llamado");
                Console.WriteLine($"[AUDIT DEBUG] UserId: {userId}");
                Console.WriteLine($"[AUDIT DEBUG] Module: {module}");
                Console.WriteLine($"[AUDIT DEBUG] Action: {action}");

                // FILTRO: Solo auditar acciones que modifican datos o accesos críticos
                if (!ShouldAudit(module, action))
                {
                    Console.WriteLine($"[AUDIT] Acción '{action}' en módulo '{module}' NO requiere auditoría - OMITIDA");
                    Console.WriteLine("=================================================");
                    return;
                }

                Console.WriteLine($"[AUDIT DEBUG] Acción '{action}' SERÁ AUDITADA");
                Console.WriteLine($"[AUDIT DEBUG] EntityId: {entityId}");
                Console.WriteLine($"[AUDIT DEBUG] Description: {description}");
                Console.WriteLine("=================================================");

                if (userId <= 0)
                {
                    Console.WriteLine($"[AUDIT ERROR] UserId inválido: {userId}. No se guardará el log.");
                    return;
                }

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Module = module,
                    Action = action,
                    EntityId = entityId,
                    EntityName = entityName,
                    Description = description,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"[AUDIT DEBUG] Objeto AuditLog creado correctamente");
                Console.WriteLine($"[AUDIT DEBUG] Llamando a _auditRepository.AddAsync...");

                var result = await _auditRepository.AddAsync(auditLog);

                Console.WriteLine($"[AUDIT DEBUG] ✓✓✓ Log guardado con ID: {result.Id}");
                Console.WriteLine("=================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("=================================================");
                Console.WriteLine($"[AUDIT ERROR] ✗✗✗ ERROR CRÍTICO AL GUARDAR LOG");
                Console.WriteLine($"[AUDIT ERROR] Message: {ex.Message}");
                Console.WriteLine($"[AUDIT ERROR] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[AUDIT ERROR] InnerException: {ex.InnerException.Message}");
                    Console.WriteLine($"[AUDIT ERROR] InnerException StackTrace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine("=================================================");
            }
        }

        /// <summary>
        /// Determina si una acción debe ser auditada según las mejores prácticas
        /// </summary>
        private bool ShouldAudit(string module, string action)
        {
            // Acciones que SIEMPRE se auditan (modifican datos o son críticas)
            var auditableActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Create",           // Creación de registros
                "Edit",             // Modificación de registros
                "Delete",           // Eliminación de registros
                "Login",            // Acceso al sistema
                "Logout",           // Cierre de sesión
                "PasswordChange",   // Cambio de contraseña
                "PermissionChange", // Cambio de permisos
                "Export",           // Exportación de datos
                "Import"            // Importación de datos
            };

            // Si la acción está en la lista de auditables, registrar
            if (auditableActions.Contains(action))
            {
                return true;
            }

            // OPCIONAL: Módulos críticos donde SÍ queremos auditar consultas (View)
            // Descomentar y ajustar según tus necesidades de cumplimiento normativo
            /*
            if (action.Equals("View", StringComparison.OrdinalIgnoreCase))
            {
                var criticalModulesForViewAudit = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "FinancialReports",  // Reportes financieros sensibles
                    // "Customer"        // Si necesitas GDPR/LOPD compliance
                };
                
                return criticalModulesForViewAudit.Contains(module);
            }
            */

            // Cualquier otra acción (como View) NO se audita
            return false;
        }

        public async Task<ServiceResult<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            try
            {
                Console.WriteLine($"[AUDIT QUERY] Consultando logs con filtros: Module={filter.Module}, Action={filter.Action}, Page={filter.Page}");

                var pagedResult = await _auditRepository.GetPagedAsync(filter);

                Console.WriteLine($"[AUDIT QUERY] Registros encontrados: {pagedResult.TotalCount}");

                var auditLogDtos = pagedResult.Items.Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    UserId = al.UserId,
                    UserName = al.User?.Username ?? "Unknown",
                    UserFullName = al.User?.FullName ?? "Unknown",
                    Module = al.Module,
                    Action = al.Action,
                    EntityId = al.EntityId,
                    EntityName = al.EntityName,
                    Description = al.Description,
                    OldValues = al.OldValues,
                    NewValues = al.NewValues,
                    IpAddress = al.IpAddress,
                    UserAgent = al.UserAgent,
                    CreatedAt = al.CreatedAt,
                    IsSuccess = al.IsSuccess,
                    ErrorMessage = al.ErrorMessage
                }).ToList();

                var result = new PagedResult<AuditLogDto>
                {
                    Items = auditLogDtos,
                    TotalCount = pagedResult.TotalCount,
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize
                };

                return ServiceResult<PagedResult<AuditLogDto>>.Success(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDIT QUERY ERROR] {ex.Message}");
                return ServiceResult<PagedResult<AuditLogDto>>.Failure($"Error al obtener registros de auditoría: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<AuditLogDto>>> GetUserActivityAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var auditLogs = await _auditRepository.GetByUserIdAsync(userId, startDate, endDate);

                var auditLogDtos = auditLogs.Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    UserId = al.UserId,
                    UserName = al.User?.Username ?? "Unknown",
                    UserFullName = al.User?.FullName ?? "Unknown",
                    Module = al.Module,
                    Action = al.Action,
                    EntityId = al.EntityId,
                    EntityName = al.EntityName,
                    Description = al.Description,
                    OldValues = al.OldValues,
                    NewValues = al.NewValues,
                    IpAddress = al.IpAddress,
                    UserAgent = al.UserAgent,
                    CreatedAt = al.CreatedAt,
                    IsSuccess = al.IsSuccess,
                    ErrorMessage = al.ErrorMessage
                }).ToList();

                return ServiceResult<List<AuditLogDto>>.Success(auditLogDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<AuditLogDto>>.Failure($"Error al obtener actividad del usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Dictionary<string, int>>> GetActivitySummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var summary = await _auditRepository.GetActivitySummaryAsync(startDate, endDate);
                return ServiceResult<Dictionary<string, int>>.Success(summary);
            }
            catch (Exception ex)
            {
                return ServiceResult<Dictionary<string, int>>.Failure($"Error al obtener resumen de actividad: {ex.Message}");
            }
        }
    }
}