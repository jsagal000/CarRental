// calendar.js - FullCalendar EXACTO como Google Calendar
let calendar = null;
let dotNetHelper = null;

window.initializeCalendar = function (eventsJson, dotNetRef) {
    console.log('[Calendar] 🚀 Inicializando...');
    dotNetHelper = dotNetRef;

    if (typeof FullCalendar === 'undefined') {
        console.error('[Calendar] ❌ ERROR: FullCalendar no disponible');
        return;
    }

    const events = JSON.parse(eventsJson);
    console.log(`[Calendar] ✅ ${events.length} eventos cargados`);

    const calendarEl = document.getElementById('calendar');
    if (!calendarEl) {
        console.error('[Calendar] ❌ ERROR: #calendar no encontrado');
        return;
    }

    if (calendar) {
        calendar.destroy();
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
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

        height: '100%',
        events: events,
        editable: true,
        eventStartEditable: true,
        eventDurationEditable: true,
        firstDay: 1,
        navLinks: true,

        // Configuración de vistas
        views: {
            dayGridMonth: {
                dayMaxEvents: true,
                displayEventTime: false
            },
            timeGridWeek: {
                slotMinTime: '06:00:00',
                slotMaxTime: '22:00:00',
                slotDuration: '01:00:00',
                allDaySlot: true,
                displayEventTime: true,
                eventTimeFormat: {
                    hour: '2-digit',
                    minute: '2-digit',
                    meridiem: false,
                    hour12: false
                }
            },
            timeGridDay: {
                slotMinTime: '06:00:00',
                slotMaxTime: '22:00:00',
                slotDuration: '00:30:00',
                allDaySlot: true,
                displayEventTime: true,
                eventTimeFormat: {
                    hour: '2-digit',
                    minute: '2-digit',
                    meridiem: false,
                    hour12: false
                }
            },
            listWeek: {
                listDayFormat: { weekday: 'long', month: 'long', day: 'numeric' },
                listDaySideFormat: false,
                noEventsContent: 'No hay eventos para mostrar'
            }
        },

        // Vista Lista personalizada
        eventContent: function (arg) {
            if (arg.view.type === 'listWeek') {
                const props = arg.event.extendedProps;
                const div = document.createElement('div');
                div.className = 'p-3 border-l-4 hover:bg-gray-50 transition-colors';
                div.style.borderColor = arg.backgroundColor;
                div.innerHTML = `
                    <div class="flex items-start justify-between gap-3">
                        <div class="flex-1">
                            <p class="font-semibold text-sm text-gray-900">
                                ${props.make || ''} ${props.model || ''}
                            </p>
                            <p class="text-xs text-gray-600 mt-1">
                                📅 ${formatDate(arg.event.start)} - ${formatDate(arg.event.end)}
                            </p>
                            <p class="text-xs text-gray-500 mt-0.5">
                                🕐 ${formatTime(arg.event.start)} - ${formatTime(arg.event.end)}
                            </p>
                            ${props.licensePlate ? `
                                <p class="text-xs text-gray-500 mt-0.5">
                                    🚗 ${props.licensePlate}
                                </p>
                            ` : ''}
                        </div>
                        <span class="text-xs px-2 py-1 rounded-full text-white font-medium whitespace-nowrap"
                              style="background-color: ${arg.backgroundColor}">
                            ${props.status || ''}
                        </span>
                    </div>
                `;
                return { domNodes: [div] };
            }
            return true;
        },

        // Click en fecha
        dateClick: function (info) {
            console.log('[Calendar] 📅 Fecha clickeada:', info.dateStr);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', info.dateStr)
                    .catch(err => console.error('[Calendar] Error:', err));
            }
        },

        // Click en evento
        eventClick: function (info) {
            info.jsEvent.preventDefault();

            const eventDate = info.event.start;
            const dateStr = eventDate.toISOString().split('T')[0];

            console.log('[Calendar] 📌 Evento clickeado:', dateStr);

            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', dateStr)
                    .catch(err => console.error('[Calendar] Error:', err));
            }
        },

        // Cambio de vista - Notificar a Blazor
        viewDidMount: function (info) {
            console.log('[Calendar] 👁️ Vista montada:', info.view.type);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnViewChange', info.view.type)
                    .catch(err => console.error('[Calendar] Error en OnViewChange:', err));
            }
        },

        // Cambio de fechas visibles
        datesSet: function (info) {
            console.log('[Calendar] 📆 Fechas actualizadas');

            // Notificar cambio de vista
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnViewChange', info.view.type)
                    .catch(err => console.error('[Calendar] Error:', err));
            }

            // Actualizar fecha seleccionada
            const currentDate = calendar.getDate();
            const dateStr = currentDate.toISOString().split('T')[0];

            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', dateStr)
                    .catch(err => console.error('[Calendar] Error:', err));
            }
        },

        // Drag & Drop
        eventDrop: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString();
            const newEnd = info.event.end ? info.event.end.toISOString() : newStart;

            if (dotNetHelper) {
                try {
                    const success = await dotNetHelper.invokeMethodAsync('OnEventDrop', rentalId, newStart, newEnd);
                    if (!success) {
                        info.revert();
                    }
                } catch (err) {
                    console.error('[Calendar] Error en Drag:', err);
                    info.revert();
                }
            } else {
                info.revert();
            }
        },

        // Resize
        eventResize: async function (info) {
            const rentalId = info.event.id;
            const newStart = info.event.start.toISOString();
            const newEnd = info.event.end ? info.event.end.toISOString() : newStart;

            if (dotNetHelper) {
                try {
                    const success = await dotNetHelper.invokeMethodAsync('OnEventDrop', rentalId, newStart, newEnd);
                    if (!success) {
                        info.revert();
                    }
                } catch (err) {
                    console.error('[Calendar] Error en Resize:', err);
                    info.revert();
                }
            } else {
                info.revert();
            }
        }
    });

    calendar.render();
    console.log('[Calendar] ✅ Renderizado exitosamente');
};

// Helpers para formatear fechas
function formatDate(date) {
    if (!date) return '';
    return new Date(date).toLocaleDateString('es-EC', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
    });
}

function formatTime(date) {
    if (!date) return '';
    return new Date(date).toLocaleTimeString('es-EC', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    });
}

// Actualizar eventos
window.updateCalendarEvents = function (eventsJson) {
    if (calendar) {
        const events = JSON.parse(eventsJson);
        calendar.removeAllEvents();
        calendar.addEventSource(events);
        calendar.refetchEvents();
        console.log('[Calendar] ✅ Eventos actualizados:', events.length);
    }
};

// Cambiar vista
window.changeCalendarView = function (viewName) {
    if (calendar) {
        calendar.changeView(viewName);
    }
};

// Ir a fecha
window.goToDate = function (dateStr) {
    if (calendar) {
        calendar.gotoDate(dateStr);
    }
};

// Obtener fecha actual
window.getCurrentDate = function () {
    if (calendar) {
        return calendar.getDate().toISOString();
    }
    return new Date().toISOString();
};

// Refrescar
window.refreshCalendar = function () {
    if (calendar) {
        calendar.refetchEvents();
    }
};

// Limpiar
window.disposeCalendar = function () {
    if (calendar) {
        calendar.destroy();
        calendar = null;
    }
    dotNetHelper = null;
};
