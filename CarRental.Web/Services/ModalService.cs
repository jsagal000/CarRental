// CarRental.Web/Services/ModalService.cs
using Microsoft.JSInterop;

namespace CarRental.Web.Services
{
    public class ModalService
    {
        private readonly IJSRuntime _jsRuntime;

        public ModalService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Muestra un modal de confirmación simple
        /// </summary>
        public async Task<bool> ConfirmAsync(string title, string message, string icon = "question")
        {
            return await _jsRuntime.InvokeAsync<bool>("Swal.fire", new
            {
                title = title,
                text = message,
                icon = icon,
                showCancelButton = true,
                confirmButtonColor = "#3085d6",
                cancelButtonColor = "#d33",
                confirmButtonText = "Sí, confirmar",
                cancelButtonText = "Cancelar"
            }).AsTask().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var result = task.Result;
                    return result;
                }
                return false;
            });
        }

        /// <summary>
        /// Modal de confirmación para eliminar
        /// </summary>
        public async Task<bool> ConfirmDeleteAsync(string itemName = "este elemento")
        {
            var result = await _jsRuntime.InvokeAsync<SweetAlertResult>("Swal.fire", new
            {
                title = "¿Estás seguro?",
                html = $"Esta acción eliminará <strong>{itemName}</strong> de forma permanente.<br><br>Esta acción <strong>no se puede deshacer</strong>.",
                icon = "warning",
                showCancelButton = true,
                confirmButtonColor = "#d33",
                cancelButtonColor = "#6c757d",
                confirmButtonText = "Sí, eliminar",
                cancelButtonText = "Cancelar",
                focusCancel = true,
                reverseButtons = true,
            });

            return result.IsConfirmed;
        }

        /// <summary>
        /// Modal de confirmación para finalizar alquiler
        /// </summary>
        public async Task<bool> ConfirmFinalizeRentalAsync()
        {
            var result = await _jsRuntime.InvokeAsync<SweetAlertResult>("Swal.fire", new
            {
                title = "Finalizar Alquiler",
                html = @"
                    <div class='text-left'>
                        <p class='mb-2'>Al finalizar este alquiler:</p>
                        <ul class='list-disc pl-5 space-y-1 text-sm'>
                            <li>Se calcularán los costos finales</li>
                            <li>El vehículo volverá a estar disponible</li>
                            <li>Se registrará la fecha de devolución</li>
                            <li>No se podrá editar posteriormente</li>
                        </ul>
                    </div>
                ",
                icon = "question",
                showCancelButton = true,
                confirmButtonColor = "#10b981",
                cancelButtonColor = "#6c757d",
                confirmButtonText = "Sí, finalizar",
                cancelButtonText = "Cancelar",
                focusCancel = true,
                reverseButtons = true
            });

            return result.IsConfirmed;
        }

        /// <summary>
        /// Modal de confirmación para cancelar alquiler con opción de calcular días
        /// </summary>
        public async Task<CancelRentalResult> ConfirmCancelRentalAsync()
        {
            var result = await _jsRuntime.InvokeAsync<SweetAlertResult>("Swal.fire", new
            {
                title = "Cancelar Alquiler",
                html = @"
                    <div class='text-left'>
                        <p class='mb-3 font-semibold'>¿Cómo deseas cancelar este alquiler?</p>
                        <div class='space-y-2'>
                            <label class='flex items-start cursor-pointer p-3 border rounded hover:bg-gray-50'>
                                <input type='radio' name='cancelOption' value='calculate' class='mt-1 mr-3' checked>
                                <div>
                                    <div class='font-medium'>Calcular días transcurridos</div>
                                    <div class='text-sm text-gray-600'>Se cobrará el costo proporcional por los días de uso</div>
                                </div>
                            </label>
                            <label class='flex items-start cursor-pointer p-3 border rounded hover:bg-gray-50'>
                                <input type='radio' name='cancelOption' value='nocost' class='mt-1 mr-3'>
                                <div>
                                    <div class='font-medium'>Cancelar sin costo</div>
                                    <div class='text-sm text-gray-600'>No se generará ningún cargo</div>
                                </div>
                            </label>
                        </div>
                    </div>
                ",
                icon = "warning",
                showCancelButton = true,
                confirmButtonColor = "#f97316",
                cancelButtonColor = "#6c757d",
                confirmButtonText = "Confirmar Cancelación",
                cancelButtonText = "Volver",
                focusCancel = true,
                reverseButtons = true,
                preConfirm = "() => { return document.querySelector('input[name=\"cancelOption\"]:checked').value; }"
            });

            if (result.IsConfirmed)
            {
                return new CancelRentalResult
                {
                    Confirmed = true,
                    CalculateDays = result.Value?.ToString() == "calculate"
                };
            }

            return new CancelRentalResult { Confirmed = false };
        }

        /// <summary>
        /// Muestra un mensaje de éxito
        /// </summary>
        public async Task ShowSuccessAsync(string title, string message = "")
        {
            await _jsRuntime.InvokeVoidAsync("Swal.fire", new
            {
                title = title,
                text = message,
                icon = "success",
                confirmButtonColor = "#10b981",
                confirmButtonText = "Aceptar"
            });
        }

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        public async Task ShowErrorAsync(string title, string message = "")
        {
            await _jsRuntime.InvokeVoidAsync("Swal.fire", new
            {
                title = title,
                text = message,
                icon = "error",
                confirmButtonColor = "#ef4444",
                confirmButtonText = "Aceptar"
            });
        }

        /// <summary>
        /// Muestra un mensaje de información
        /// </summary>
        public async Task ShowInfoAsync(string title, string message = "")
        {
            await _jsRuntime.InvokeVoidAsync("Swal.fire", new
            {
                title = title,
                text = message,
                icon = "info",
                confirmButtonColor = "#3b82f6",
                confirmButtonText = "Aceptar"
            });
        }

        /// <summary>
        /// Muestra un mensaje con input de texto
        /// </summary>
        public async Task<string?> ShowInputAsync(string title, string placeholder = "", string inputValue = "")
        {
            var result = await _jsRuntime.InvokeAsync<SweetAlertResult>("Swal.fire", new
            {
                title = title,
                input = "text",
                inputValue = inputValue,
                inputPlaceholder = placeholder,
                showCancelButton = true,
                confirmButtonText = "Aceptar",
                cancelButtonText = "Cancelar",
                inputValidator = "(value) => { if (!value) { return 'Este campo es requerido'; } }"
            });

            return result.IsConfirmed ? result.Value?.ToString() : null;
        }

        /// <summary>
        /// Muestra un modal con loading
        /// </summary>
        public async Task ShowLoadingAsync(string title = "Procesando...")
        {
            await _jsRuntime.InvokeVoidAsync("Swal.fire", new
            {
                title = title,
                allowOutsideClick = false,
                allowEscapeKey = false,
                allowEnterKey = false,
                showConfirmButton = false,
                willOpen = "() => { Swal.showLoading(); }"
            });
        }

        /// <summary>
        /// Cierra el modal actual
        /// </summary>
        public async Task CloseAsync()
        {
            await _jsRuntime.InvokeVoidAsync("Swal.close");
        }
    }

    // Clases auxiliares
    public class SweetAlertResult
    {
        public bool IsConfirmed { get; set; }
        public bool IsDismissed { get; set; }
        public object? Value { get; set; }
    }

    public class CancelRentalResult
    {
        public bool Confirmed { get; set; }
        public bool CalculateDays { get; set; }
    }
}