// calendar.js - Versión simplificada sin layout absoluto
let calendar = null;
let dotNetHelper = null;

window.initializeCalendar = function (eventsJson, dotNetRef) {
    console.log('[Calendar] 🚀 Inicializando...');
    dotNetHelper = dotNetRef;

    if (typeof FullCalendar === 'undefined') {
        console.error('[Calendar] ❌ FullCalendar no disponible');
        return;
    }

    const events = JSON.parse(eventsJson);
    console.log(`[Calendar] ✅ ${events.length} eventos cargados`);

    const calendarEl = document.getElementById('calendar');
    if (!calendarEl) {
        console.error('[Calendar] ❌ #calendar no encontrado');
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
        nowIndicator: true,

        views: {
            dayGridMonth: {
                dayMaxEvents: 3,
                displayEventTime: false,
                fixedWeekCount: false,
                titleFormat: { month: 'long', year: 'numeric' }
            },
            timeGridWeek: {
                slotMinTime: '00:00:00',
                slotMaxTime: '24:00:00',
                slotDuration: '01:00:00',
                allDaySlot: false,
                displayEventTime: true,
                slotLabelInterval: '01:00',
                scrollTime: '06:00:00',
                eventTimeFormat: {
                    hour: 'numeric',
                    minute: '2-digit',
                    hour12: true
                },
                titleFormat: { month: 'short', year: 'numeric' },
                dayHeaderContent: function (arg) {
                    const dayName = arg.date.toLocaleDateString('es-EC', { weekday: 'short' }).toUpperCase();
                    const dayNumber = arg.date.getDate();
                    return {
                        html: '<div style="text-align: center;"><div style="font-size: 0.9em;">' + dayName + '</div><div style="font-size: 1.3em; font-weight: bold;">' + dayNumber + '</div></div>'
                    };
                }
            },
            timeGridDay: {
                slotMinTime: '00:00:00',
                slotMaxTime: '24:00:00',
                slotDuration: '01:00:00',
                allDaySlot: false,
                displayEventTime: true,
                slotLabelInterval: '01:00',
                scrollTime: '06:00:00',
                eventTimeFormat: {
                    hour: 'numeric',
                    minute: '2-digit',
                    hour12: true
                },
                titleFormat: { month: 'short', year: 'numeric' }
            },
            listWeek: {
                listDayFormat: { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' },
                noEventsContent: 'No hay eventos para mostrar',
                titleFormat: { day: 'numeric', month: 'short', year: 'numeric' }
            }
        },

        slotLabelContent: function (arg) {
            const hour = arg.date.getHours();
            let hour12 = hour % 12 || 12;
            let label = hour12.toString();

            if (hour === 0 || hour === 12) {
                label += ' ' + (hour < 12 ? 'AM' : 'PM');
            }

            return { html: '<span style="font-size: 0.85em;">' + label + '</span>' };
        },

        eventContent: function (arg) {
            if (arg.view.type === 'listWeek') {
                const props = arg.event.extendedProps;
                const div = document.createElement('div');
                div.className = 'p-3 border-l-4 hover:bg-gray-50';
                div.style.borderColor = arg.backgroundColor;
                div.innerHTML = `
                    <div class="flex justify-between gap-3">
                        <div style="flex: 1;">
                            <p class="font-semibold text-sm">${props.make || ''} ${props.model || ''}</p>
                            <p class="text-xs text-gray-600">📅 ${formatDate(arg.event.start)} - ${formatDate(arg.event.end)}</p>
                            <p class="text-xs text-gray-500">🕐 ${formatTime(arg.event.start)} - ${formatTime(arg.event.end)}</p>
                            ${props.licensePlate ? `<p class="text-xs text-gray-500">🚗 ${props.licensePlate}</p>` : ''}
                        </div>
                        <span class="text-xs px-2 py-1 rounded text-white" style="background-color: ${arg.backgroundColor}">
                            ${props.status}
                        </span>
                    </div>
                `;
                return { domNodes: [div] };
            }
            return true;
        },

        dateClick: function (info) {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', info.dateStr)
                    .catch(err => console.error('[Calendar] Error:', err));
            }
        },

        eventClick: function (info) {
            info.jsEvent.preventDefault();
            const dateStr = info.event.start.toISOString().split('T')[0];
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDateClick', dateStr)
                    .catch(err => console.error('[Calendar] Error:', err));
            }
        },

        viewDidMount: function (info) {
            console.log('[Calendar] 👁️ Vista:', info.view.type);

            // Controlar sidebar
            const sidebar = document.getElementById('events-sidebar');
            const wrapper = document.getElementById('calendar-wrapper');

            if (sidebar && wrapper) {
                if (info.view.type === 'dayGridMonth') {
                    sidebar.style.display = 'flex';
                    wrapper.style.right = '320px';
                } else {
                    sidebar.style.display = 'none';
                    wrapper.style.right = '0';
                }

                setTimeout(() => calendar.updateSize(), 100);
            }
        },

        eventDrop: async function (info) {
            if (dotNetHelper) {
                try {
                    const success = await dotNetHelper.invokeMethodAsync(
                        'OnEventDrop',
                        info.event.id,
                        info.event.start.toISOString(),
                        info.event.end ? info.event.end.toISOString() : info.event.start.toISOString()
                    );
                    if (!success) info.revert();
                } catch (err) {
                    console.error('[Calendar] Error:', err);
                    info.revert();
                }
            } else {
                info.revert();
            }
        },

        eventResize: async function (info) {
            if (dotNetHelper) {
                try {
                    const success = await dotNetHelper.invokeMethodAsync(
                        'OnEventDrop',
                        info.event.id,
                        info.event.start.toISOString(),
                        info.event.end ? info.event.end.toISOString() : info.event.start.toISOString()
                    );
                    if (!success) info.revert();
                } catch (err) {
                    console.error('[Calendar] Error:', err);
                    info.revert();
                }
            } else {
                info.revert();
            }
        }
    });

    calendar.render();
    console.log('[Calendar] ✅ Renderizado');
};

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
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });
}

window.updateCalendarEvents = function (eventsJson) {
    if (calendar) {
        const events = JSON.parse(eventsJson);
        calendar.removeAllEvents();
        calendar.addEventSource(events);
        calendar.refetchEvents();
    }
};

window.refreshCalendar = function () {
    if (calendar) calendar.refetchEvents();
};

window.disposeCalendar = function () {
    if (calendar) {
        calendar.destroy();
        calendar = null;
    }
    dotNetHelper = null;
};
