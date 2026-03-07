// modal-helpers.js - Funciones auxiliares para ModalService

// Mostrar modal de loading
window.showLoadingModal = function (title) {
    Swal.fire({
        title: title || 'Procesando...',
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
};

// Modal de cancelar alquiler con radio buttons
window.showCancelRentalModal = async function () {
    const result = await Swal.fire({
        title: 'Cancelar Alquiler',
        html: `
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
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f97316',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Confirmar Cancelación',
        cancelButtonText: 'Volver',
        focusCancel: true,
        reverseButtons: true,
        preConfirm: () => {
            const selectedOption = document.querySelector('input[name="cancelOption"]:checked');
            if (selectedOption) {
                return selectedOption.value;
            }
            return null;
        }
    });

    if (result.isConfirmed && result.value) {
        return result.value;
    }

    return null;
};
