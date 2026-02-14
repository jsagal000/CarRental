// CarRental.Api/Controllers/ContractController.cs
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Forms; // Necesario para PdfAcroForm
using iText.Forms.Fields; // Necesario para PdfFormField
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting; // Necesario para IWebHostEnvironment
using System.Collections.Generic; // Necesario para IDictionary (aunque no se usa directamente para pasar fields)
using System.Linq;

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        private readonly IWebHostEnvironment _hostingEnvironment; // Inyectado para acceder a wwwroot
        private readonly ILogger<ContractController> _logger;

        public ContractController(IRentalService rentalService, IWebHostEnvironment hostingEnvironment, ILogger<ContractController> logger)
        {
            _rentalService = rentalService;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Genera el contrato de alquiler en formato PDF rellenando una plantilla existente.
        /// </summary>
        /// <param name="rentalId">El ID del alquiler para el cual generar el contrato.</param>
        /// <returns>Un archivo PDF.</returns>
        [HttpGet("generate/{rentalId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateRentalContractPdf(int rentalId)
        {
            _logger.LogInformation("Solicitud para generar contrato PDF (desde plantilla) para RentalId: {RentalId}", rentalId);
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(rentalId);

                if (rental == null)
                {
                    _logger.LogWarning("Alquiler con ID {RentalId} no encontrado para generar contrato desde plantilla.", rentalId);
                    return NotFound($"Alquiler con ID {rentalId} no encontrado.");
                }

                // Ruta al archivo de plantilla PDF en wwwroot/Templates
                string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "Templates", "Contrato_Template.pdf");

                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogError("Archivo de plantilla PDF no encontrado en: {TemplatePath}", templatePath);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error: La plantilla del contrato no se encontró en el servidor.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    // Llama al método para rellenar la plantilla
                    FillPdfTemplate(templatePath, memoryStream, rental);
                    _logger.LogInformation("Contrato PDF (desde plantilla) generado exitosamente para RentalId: {RentalId}.", rentalId);

                    return new FileContentResult(memoryStream.ToArray(), "application/pdf")
                    {
                        FileDownloadName = $"Contrato_Alquiler_{rental.Id}.pdf"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el contrato PDF (desde plantilla) para RentalId: {RentalId}", rentalId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno del servidor al generar el contrato: {ex.Message}");
            }
        }

        /// <summary>
        /// Rellena una plantilla PDF existente con los datos del alquiler.
        /// </summary>
        /// <param name="templatePath">Ruta física al archivo de plantilla PDF.</param>
        /// <param name="outputStream">Stream donde se escribirá el PDF resultante.</param>
        /// <param name="rental">El objeto Rental con los datos a rellenar.</param>
        private void FillPdfTemplate(string templatePath, Stream outputStream, Rental rental)
        {
            // Read the existing PDF document
            PdfDocument pdfDocument = null;
            try
            {
                pdfDocument = new PdfDocument(new PdfReader(templatePath), new PdfWriter(outputStream));
                var form = PdfAcroForm.GetAcroForm(pdfDocument, true); // `true` flattens the form fields

                // Calcular los días de alquiler
                TimeSpan duration = rental.EndDate - rental.StartDate;
                int rentalDays = (int)Math.Ceiling(duration.TotalHours / 24.0);
                if (rentalDays <= 0) rentalDays = 1; // Mínimo 1 día

                // --- Rellenar los campos de texto del formulario ---
                // Los nombres de los campos (ej. "arrendatario_nombre_apellido") DEBEN COINCIDIR EXACTAMENTE
                // con los nombres que les diste en tu herramienta de edición de PDF (ej. Adobe Acrobat Pro).
                // Si un nombre de campo es incorrecto, iText7 no lo encontrará y se registrará una advertencia.

                // NOTA: Ahora pasamos el objeto 'form' directamente a las funciones de SetField
                SetTextField(form, "ciudad", "Guayaquil"); // Asumiendo ciudad fija
                SetTextField(form, "fecha_contrato", rental.CreatedDate.ToString("dd/MM/yyyy"));

                // Datos del Arrendatario (Customer)
                SetTextField(form, "arrendatario_nombre_apellido", $"{rental.Customer?.FirstName} {rental.Customer?.LastName}");
                SetTextField(form, "arrendatario_telefono_fijo", rental.Customer?.PhoneNumber);
                SetTextField(form, "arrendatario_tipo_documento", rental.Customer?.TypeOfDocument.ToString());
                SetTextField(form, "arrendatario_numero_documento", rental.Customer?.DocumentNumber);
                SetTextField(form, "arrendatario_telefono_movil", rental.Customer?.PhoneNumber);
                SetTextField(form, "arrendatario_direccion_domicilio", $"{rental.Customer?.Address}, {rental.Customer?.City}");
                SetTextField(form, "arrendatario_correo_electronico", rental.Customer?.Email);

                // Datos del Conductor (del objeto Rental)
                SetTextField(form, "conductor_nombre", rental.DriverName);
                SetTextField(form, "conductor_licencia_tipo", rental.DriverLicenseType.ToString());
                SetTextField(form, "conductor_licencia_caducidad", rental.DriverLicenseExpirationDate.ToString("dd/MM/yyyy"));

                // Tabla de Tarifas
                SetTextField(form, "no_de_dias", rentalDays.ToString());
                SetTextField(form, "valor_del_dia", rental.DailyRate.ToString("0.00")); // Sin símbolo de moneda para el campo numérico
                SetTextField(form, "valor_total_tarifa", rental.TotalCost.ToString("0.00")); // Costo Total de la tarifa

                SetTextField(form, "suma_total_tarifa", rental.TotalCost.ToString("0.00")); // El campo del PDF es "SUMA TOTAL $"

                // Características del Vehículo
                SetTextField(form, "vehiculo_marca", rental.Vehicle?.Make);
                SetTextField(form, "vehiculo_modelo", rental.Vehicle?.Model);
                SetTextField(form, "vehiculo_placa", rental.Vehicle?.LicensePlate);
                SetTextField(form, "vehiculo_color", rental.Vehicle?.Color);
                SetTextField(form, "observaciones_generales", rental.Notes ?? "N/A"); // Usar el campo de notas del alquiler

                // Constancia de Entrega y Recepción - Datos Clave
                SetTextField(form, "entrega_placa", rental.Vehicle?.LicensePlate);
                SetTextField(form, "entrega_fecha", rental.StartDate.ToString("dd/MM/yyyy"));
                SetTextField(form, "entrega_hora", rental.StartDate.ToString("HH:mm"));
                SetTextField(form, "entrega_dias_alquilado", rentalDays.ToString());
                SetTextField(form, "kilometraje_entregado_recibo", rental.MileageAtDelivery.ToString());

                SetTextField(form, "recepcion_fecha", rental.ActualReturnDate?.ToString("dd/MM/yyyy") ?? "___/___/____");
                SetTextField(form, "recepcion_hora", rental.ActualReturnDate?.ToString("HH:mm") ?? "__:__");
                SetTextField(form, "kilometraje_recepcion_recibo", (rental.MileageAtReturn ?? 0).ToString());
                SetTextField(form, "kilometro_recorrido", (rental.MileageAtReturn - rental.MileageAtDelivery)?.ToString() ?? "________"); // Se asume cálculo

            }
            finally
            {
                // Asegúrate de cerrar el documento en el bloque finally
                if (pdfDocument != null)
                {
                    pdfDocument.Close();
                }
            }
        }

        // --- Métodos de Ayuda para rellenar campos ---

        /// <summary>
        /// Establece el valor de un campo de texto en el formulario PDF.
        /// </summary>
        /// <param name="form">El objeto PdfAcroForm que contiene los campos.</param>
        /// <param name="fieldName">Nombre del campo de texto en el PDF.</param>
        /// <param name="value">Valor a establecer en el campo.</param>
        private void SetTextField(PdfAcroForm form, string fieldName, string value)
        {
            PdfFormField field = form.GetField(fieldName); // Acceder al campo directamente por nombre
            if (field != null)
            {
                if (field.GetFormType().Equals("Tx")) // "Tx" is the string representation for a Text field in PDF forms
                {
                    field.SetValue(value);
                }
                else
                {
                    _logger.LogWarning("Campo '{FieldName}' no es un campo de texto en la plantilla PDF. Ignorado.", fieldName);
                }
            }
            else
            {
                _logger.LogWarning("Campo de formulario PDF '{FieldName}' no encontrado en la plantilla.", fieldName);
            }
        }

        /// <summary>
        /// Establece el estado (marcado/desmarcado) de un campo de casilla de verificación (checkbox) en el formulario PDF.
        /// </summary>
        /// <param name="form">El objeto PdfAcroForm que contiene los campos.</param>
        /// <param name="fieldName">Nombre del campo de casilla de verificación en el PDF.</param>
        /// <param name="isChecked">True para marcar, False para desmarcar.</param>
    }
}
