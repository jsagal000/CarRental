using Microsoft.AspNetCore.Mvc;
using System.IO; // Para Path y File
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Para IFormFile

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")] // Ruta base: /api/Files
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<FilesController> _logger; // Para logging

        // Inyectamos IWebHostEnvironment para acceder a la ruta física de la aplicación
        // y ILogger para registrar eventos.
        public FilesController(IWebHostEnvironment hostingEnvironment, ILogger<FilesController> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // POST: api/Files/upload/vehicle-images
        // Este endpoint recibirá una o más imágenes.
        [HttpPost("upload/vehicle-images")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))] // Retorna una lista de URLs
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadVehicleImages(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                _logger.LogWarning("No se han proporcionado archivos para subir.");
                return BadRequest("No se han proporcionado archivos para subir.");
            }

            // Definimos la ruta base para los archivos subidos.
            // Usamos WebRootPath si está disponible; de lo contrario, usamos ContentRootPath.
            // Esta lógica DEBE IR PRIMERO para que 'basePath' nunca sea null al combinarse.
            string basePath = _hostingEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = _hostingEnvironment.ContentRootPath;
                _logger.LogWarning("WebRootPath es nulo. Usando ContentRootPath para la carpeta de subidas: {Path}", basePath);
            }

            // Ahora, construimos la ruta completa de la carpeta de subidas usando basePath
            string uploadsFolder = Path.Combine(basePath, "Uploads", "VehicleImages");

            // Creamos la carpeta si no existe
            if (!Directory.Exists(uploadsFolder))
            {
                _logger.LogInformation("Creando directorio de subidas: {Path}", uploadsFolder);
                try
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Permiso denegado al intentar crear el directorio de subidas: {Path}", uploadsFolder);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Permiso denegado al intentar guardar archivos. Por favor, contacte al administrador.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear el directorio de subidas: {Path}", uploadsFolder);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor al preparar la subida de archivos.");
                }
            }

            List<string> fileUrls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    _logger.LogWarning("Archivo vacío detectado.");
                    continue; // Saltar archivos vacíos
                }

                // Generar un nombre de archivo único para evitar colisiones
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    // Guardar el archivo en el sistema de archivos
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Generar la URL relativa para acceder al archivo desde el cliente Blazor
                    string fileUrl = $"/Uploads/VehicleImages/{uniqueFileName}";
                    fileUrls.Add(fileUrl);
                    _logger.LogInformation("Archivo '{FileName}' guardado como '{UniqueFileName}' en {Path}. URL: {Url}", file.FileName, uniqueFileName, filePath, fileUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar el archivo '{FileName}' en {Path}", file.FileName, filePath);
                    // Opcional: Podrías devolver un error específico para este archivo
                    // o simplemente continuar y reportar que algunos archivos no se subieron.
                    // Por ahora, lo registramos y no lo incluimos en las URLs de éxito.
                }
            }

            if (fileUrls.Any())
            {
                // Devolver las URLs de los archivos subidos exitosamente
                return Ok(fileUrls);
            }
            else
            {
                // Si no se pudo subir ningún archivo
                return BadRequest("No se pudieron subir los archivos proporcionados.");
            }
        }
    }
    }
