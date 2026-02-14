using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanySettingsController : ControllerBase
    {
        private readonly ICompanySettingsService _companySettingsService;
        private readonly ILogger<CompanySettingsController> _logger;

        public CompanySettingsController(
            ICompanySettingsService companySettingsService,
            ILogger<CompanySettingsController> logger)
        {
            _companySettingsService = companySettingsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanySettings()
        {
            try
            {
                _logger.LogInformation("Obteniendo configuración de la empresa");

                var result = await _companySettingsService.GetCompanySettingsAsync();

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Configuración obtenida exitosamente");
                    return Ok(result);
                }

                _logger.LogWarning("Error al obtener configuración: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener configuración de empresa");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompanySettings([FromBody] CompanySettings settings)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar configuración");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Actualizando configuración de la empresa");

                var result = await _companySettingsService.UpdateCompanySettingsAsync(settings);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Configuración actualizada exitosamente");
                    return Ok(result);
                }

                _logger.LogWarning("Error al actualizar configuración: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar configuración de empresa");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("upload-logo")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit] // Opcional: permite archivos más grandes
        public async Task<IActionResult> UploadLogo(IFormFile logo)
        {
            try
            {
                if (logo == null || logo.Length == 0)
                {
                    _logger.LogWarning("Intento de subir logo sin archivo");
                    return BadRequest(new { message = "No se proporcionó ningún archivo" });
                }

                // Validar que sea una imagen
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Intento de subir archivo con extensión no permitida: {Extension}", extension);
                    return BadRequest(new { message = "Solo se permiten archivos de imagen (jpg, jpeg, png, gif)" });
                }

                // Validar tamaño (máximo 2MB)
                if (logo.Length > 2 * 1024 * 1024)
                {
                    _logger.LogWarning("Intento de subir archivo que excede 2MB: {Size} bytes", logo.Length);
                    return BadRequest(new { message = "El archivo no debe exceder 2MB" });
                }

                _logger.LogInformation("Subiendo logo ({Size} bytes, extensión: {Extension})", logo.Length, extension);

                using var memoryStream = new MemoryStream();
                await logo.CopyToAsync(memoryStream);
                var logoData = memoryStream.ToArray();

                var result = await _companySettingsService.UploadLogoAsync(logoData);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Logo actualizado exitosamente");
                    return Ok(new { message = "Logo actualizado exitosamente" });
                }

                _logger.LogWarning("Error al subir logo: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al subir logo");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }
    }
}