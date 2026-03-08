// calendar.js - Inicialización de FullCalendar (Estilo Google Calendar)
let calendar = null;
let dotNetHelper = null;

// Inicializar el calendario
window.initializeCalendar = function (eventsJson, dotNetRef) {
    console.log('[Calendar] Inicializando calendario');
    dotNetHelper = dotNetRef;

    // Validar que FullCalendar esté cargado
    if (typeof FullCalendar === 'undefined') {
        console.error('[Calendar] ERROR: FullCalendar no está cargado');
        return;
    }

    const events = JSON.parse(eventsJson);
    console.log(`[Calendar] Eventos cargados: ${events.length}`);

    const calendarEl = document.getElementById('calendar');

    if (!calendarEl) {
        console.error('[Calendar] ERROR: Elemento #calendar no encontrado');
        return;
    }

    // Destruir calendario anterior si existe
    if (calendar) {
        console.log('[Calendar] Destruyendo calendario anterior');
        calendar.destroy();
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
        // Configuración básica
        initialView: 'dayGridMonth',
        locale: 'es',

        // Header sin vista Lista
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },

        buttonText: {
            today: 'Hoy',
            month: 'Mes',
            week: 'Semana',
            day: 'Día'
        },

        // Altura automática
        height: 'auto',

        // Eventos
        events: events,

        // Configuración de eventos
        editable: true,
        eventStartEditable: true,
        eventDurationEditable: true,
        eventResizableFromStart: true,

        // Estilo de eventos
        eventDisplay: 'block',
        displayEventTime: false, // No mostrar hora en eventos de todo el día

        // Configuración de días
        firstDay: 1, // Lunes como primer día
        weekNumbers: false,
        navLinks: true,

        // Configuración de tiempo para vistas de semana/día
        slotMinTime: '06:00:00',
        slotMaxTime: '22:00:00',
        slotDuration: '01:00:00',

        // Permitir selección de fechas
        selectable: false, // Deshabilitado para evitar crear eventos arrastrando
        selectMirror: false,

        // Eventos de interacción

        // Click en fecha (día del calendario)
        dateClick: function (info) {
            console.log('[Calendar] Click en fecha:', info.dateStr);

            // Notificar a Blazor que se seleccionó una fecha
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateSelect', info.dateStr);
            }
        },

        // Click en evento
        eventClick: function (info) {
            info.jsEvent.preventDefault();

            const rentalId = info.event.id;
            console.log('[Calendar] Click en evento:', rentalId);

            // Navegar a detalles
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnEventClick', rentalId);
            }
        },

        // Drag & Drop de eventos
        eventDrop: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString().split('T')[0]; // Solo fecha
            const newEnd = info.event.end ? info.event.end.toISOString().split('T')[0] : newStart;

            console.log('[Calendar] Evento movido:', rentalId, newStart, newEnd);

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

        // Resize de eventos
        eventResize: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString().split('T')[0];
            const newEnd = info.event.end ? info.event.end.toISOString().split('T')[0] : newStart;

            console.log('[Calendar] Evento redimensionado:', rentalId, newStart, newEnd);

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

            // Tooltip HTML (se muestra al pasar el mouse)
            info.el.setAttribute('title',
                `${info.event.title}\n` +
                `Estado: ${props.status}\n` +
                `${props.licensePlate ? 'Placa: ' + props.licensePlate + '\n' : ''}` +
                `${props.dailyRate ? 'Tarifa: $' + props.dailyRate + '/día' : ''}`
            );

            info.el.style.cursor = 'pointer';
        },

        // Configuración responsive
        windowResize: function (view) {
            // El layout responsive se maneja con CSS Grid en Blazor
        }
    });

    calendar.render();
    console.log('[Calendar] ✅ Calendario renderizado exitosamente');
};

// Cambiar vista del calendario
window.changeCalendarView = function (viewName) {
    if (calendar) {
        calendar.changeView(viewName);
        console.log('[Calendar] Vista cambiada a:', viewName);
    }
};

// Actualizar eventos del calendario
window.updateCalendarEvents = function (eventsJson) {
    if (calendar) {
        const events = JSON.parse(eventsJson);
        calendar.removeAllEvents();
        calendar.addEventSource(events);
        console.log('[Calendar] Eventos actualizados:', events.length);
    }
};

// Ir a una fecha específica
window.goToDate = function (dateStr) {
    if (calendar) {
        calendar.gotoDate(dateStr);
        console.log('[Calendar] Navegado a fecha:', dateStr);
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
        console.log('[Calendar] Calendario refrescado');
    }
};

// Limpiar al destruir el componente
window.disposeCalendar = function () {
    if (calendar) {
        calendar.destroy();
        calendar = null;
        console.log('[Calendar] Calendario destruido');
    }
    dotNetHelper = null;
};
