using CarRental.Api.Attributes;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermission("Audit", "View")]
        public async Task<ActionResult<ApiResult<PagedResult<AuditLogDto>>>> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
        {
            try
            {
                // Asegurar valores por defecto
                filter ??= new AuditLogFilterDto();
                filter.Page = filter.Page <= 0 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize <= 0 ? 50 : filter.PageSize;

                _logger.LogInformation($"[AUDIT QUERY] Consultando logs - Página: {filter.Page}, Módulo: {filter.Module ?? "Todos"}, Acción: {filter.Action ?? "Todas"}");

                var result = await _auditService.GetAuditLogsAsync(filter);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"[AUDIT QUERY] Error: {result.ErrorMessage}");
                    return BadRequest(ApiResult<PagedResult<AuditLogDto>>.Failure(result.ErrorMessage));
                }

                _logger.LogInformation($"[AUDIT QUERY] Encontrados {result.Data.TotalCount} registros");
                return Ok(ApiResult<PagedResult<AuditLogDto>>.Success(result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUDIT QUERY] Error inesperado");
                return StatusCode(500, ApiResult<PagedResult<AuditLogDto>>.Failure($"Error interno: {ex.Message}"));
            }
        }

        [HttpGet("user/{userId}")]
        [RequirePermission("Audit", "View")]
        public async Task<ActionResult<ApiResult<List<AuditLogDto>>>> GetUserActivity(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _auditService.GetUserActivityAsync(userId, startDate, endDate);

                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResult<List<AuditLogDto>>.Failure(result.ErrorMessage));
                }

                return Ok(ApiResult<List<AuditLogDto>>.Success(result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AUDIT QUERY] Error al obtener actividad del usuario {userId}");
                return StatusCode(500, ApiResult<List<AuditLogDto>>.Failure($"Error interno: {ex.Message}"));
            }
        }

        [HttpGet("summary")]
        [RequirePermission("Audit", "View")]
        public async Task<ActionResult<ApiResult<Dictionary<string, int>>>> GetActivitySummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _auditService.GetActivitySummaryAsync(startDate, endDate);

                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResult<Dictionary<string, int>>.Failure(result.ErrorMessage));
                }

                return Ok(ApiResult<Dictionary<string, int>>.Success(result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUDIT QUERY] Error al obtener resumen");
                return StatusCode(500, ApiResult<Dictionary<string, int>>.Failure($"Error interno: {ex.Message}"));
            }
        }
    }
}