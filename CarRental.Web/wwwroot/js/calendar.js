// calendar.js - Inicialización de FullCalendar
let calendar = null;
let dotNetHelper = null;

// Inicializar el calendario
window.initializeCalendar = function (eventsJson, dotNetRef) {
    dotNetHelper = dotNetRef;

    const events = JSON.parse(eventsJson);
    const calendarEl = document.getElementById('calendar');

    if (!calendarEl) {
        console.error('Elemento #calendar no encontrado');
        return;
    }

    // Destruir calendario anterior si existe
    if (calendar) {
        calendar.destroy();
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
        // Configuración básica
        initialView: 'dayGridMonth',
        locale: 'es',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        buttonText: {
            today: 'Hoy',
            month: 'Mes',
            week: 'Semana',
            day: 'Día',
            list: 'Lista'
        },

        // Altura automática
        height: 'auto',

        // Eventos
        events: events,

        // Configuración de eventos
        editable: true, // Permitir drag & drop
        eventStartEditable: true, // Permitir mover eventos
        eventDurationEditable: true, // Permitir cambiar duración
        eventResizableFromStart: true,

        // Estilo de eventos
        eventDisplay: 'block',
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            meridiem: false,
            hour12: false
        },

        // Configuración de días
        firstDay: 1, // Lunes como primer día
        weekNumbers: false,
        navLinks: true,

        // Configuración de tiempo
        slotMinTime: '06:00:00',
        slotMaxTime: '22:00:00',
        slotDuration: '01:00:00',

        // Configuración responsive
        windowResize: function (view) {
            if (window.innerWidth < 768) {
                calendar.changeView('listWeek');
            }
        },

        // Eventos de interacción
        eventClick: function (info) {
            info.jsEvent.preventDefault();

            const rentalId = info.event.id;
            const props = info.event.extendedProps;

            // Mostrar modal con detalles
            showEventDetails(info.event);

            // Notificar a Blazor
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnEventClick', rentalId);
            }
        },

        dateClick: function (info) {
            // Crear nueva reserva en la fecha clickeada
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', info.dateStr);
            }
        },

        eventDrop: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString();
            const newEnd = info.event.end ? info.event.end.toISOString() : newStart;

            if (dotNetHelper) {
                const success = await dotNetHelper.invokeMethodAsync(
                    'OnEventDrop',
                    rentalId,
                    newStart,
                    newEnd
                );

                if (!success) {
                    // Revertir cambios si falló
                    info.revert();
                }
            } else {
                info.revert();
            }
        },

        eventResize: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString();
            const newEnd = info.event.end ? info.event.end.toISOString() : newStart;

            if (dotNetHelper) {
                const success = await dotNetHelper.invokeMethodAsync(
                    'OnEventDrop',
                    rentalId,
                    newStart,
                    newEnd
                );

                if (!success) {
                    info.revert();
                }
            } else {
                info.revert();
            }
        },

        // Renderizado de eventos
        eventDidMount: function (info) {
            // Agregar tooltip con información
            const props = info.event.extendedProps;
            const tooltip = `
                <div style="font-size: 12px;">
                    <strong>${info.event.title}</strong><br>
                    ${props.licensePlate ? `Placa: ${props.licensePlate}<br>` : ''}
                    Cliente: ${props.customerName || 'N/A'}<br>
                    Estado: ${props.status || 'N/A'}<br>
                    ${props.dailyRate ? `Tarifa: $${props.dailyRate}/día<br>` : ''}
                    ${props.totalCost ? `Total: $${props.totalCost}` : ''}
                </div>
            `;

            info.el.setAttribute('title', tooltip);
            info.el.style.cursor = 'pointer';
        }
    });

    calendar.render();

    // Ajustar vista en móvil
    if (window.innerWidth < 768) {
        calendar.changeView('listWeek');
    }
};

// Mostrar detalles del evento en modal
function showEventDetails(event) {
    const props = event.extendedProps;

    const formatDate = (date) => {
        return new Date(date).toLocaleString('es-EC', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const details = `
        <div class="p-4">
            <h3 class="text-lg font-bold mb-2">${event.title}</h3>
            <div class="space-y-2 text-sm">
                ${props.licensePlate ? `<p><strong>Placa:</strong> ${props.licensePlate}</p>` : ''}
                <p><strong>Cliente:</strong> ${props.customerName || 'N/A'}</p>
                <p><strong>Estado:</strong> <span class="px-2 py-1 rounded text-white" style="background-color: ${event.backgroundColor}">${props.status}</span></p>
                <p><strong>Inicio:</strong> ${formatDate(event.start)}</p>
                <p><strong>Fin:</strong> ${formatDate(event.end)}</p>
                ${props.dailyRate ? `<p><strong>Tarifa diaria:</strong> $${props.dailyRate}</p>` : ''}
                ${props.totalCost ? `<p><strong>Costo total:</strong> $${props.totalCost}</p>` : ''}
            </div>
            <div class="mt-4 flex justify-end">
                <button onclick="closeEventDetails()" class="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600 mr-2">
                    Cerrar
                </button>
                <a href="/rentals/details/${event.id}" class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
                    Ver Detalles
                </a>
            </div>
        </div>
    `;

    if (window.Swal) {
        window.Swal.fire({
            html: details,
            showConfirmButton: false,
            width: '500px',
            customClass: {
                popup: 'rounded-xl'
            }
        });
    }
}

window.closeEventDetails = function () {
    if (window.Swal) {
        window.Swal.close();
    }
};

// Cambiar vista del calendario
window.changeCalendarView = function (viewName) {
    if (calendar) {
        calendar.changeView(viewName);
    }
};

// Actualizar eventos del calendario
window.updateCalendarEvents = function (eventsJson) {
    if (calendar) {
        const events = JSON.parse(eventsJson);
        calendar.removeAllEvents();
        calendar.addEventSource(events);
    }
};

// Ir a una fecha específica
window.goToDate = function (dateStr) {
    if (calendar) {
        calendar.gotoDate(dateStr);
    }
};

// Obtener fecha actual del calendario
window.getCurrentDate = function () {
    if (calendar) {
        return calendar.getDate().toISOString();
    }
    return new Date().toISOString();
};

// Refrescar calendario
window.refreshCalendar = function () {
    if (calendar) {
        calendar.refetchEvents();
    }
};

// Limpiar al destruir el componente
window.disposeCalendar = function () {
    if (calendar) {
        calendar.destroy();
        calendar = null;
    }
    dotNetHelper = null;
};
