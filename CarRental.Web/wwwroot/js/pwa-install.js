// pwa-install.js - Manejo de instalación de PWA

let deferredPrompt;
let isInstallable = false;

// Escuchar el evento beforeinstallprompt
window.addEventListener('beforeinstallprompt', (e) => {
    console.log('📱 PWA: App es instalable');

    // Prevenir el prompt automático
    e.preventDefault();

    // Guardar el evento para usarlo después
    deferredPrompt = e;
    isInstallable = true;

    // Mostrar el botón de instalación
    window.dispatchEvent(new Event('pwa-installable'));
});

// Detectar cuando la app se instala
window.addEventListener('appinstalled', (e) => {
    console.log('✅ PWA: App instalada exitosamente');
    deferredPrompt = null;
    isInstallable = false;

    // Opcional: Mostrar mensaje de éxito
    if (window.Toast) {
        window.Toast.fire({
            icon: 'success',
            title: '¡Aplicación instalada!'
        });
    }
});

// Función para verificar si la app puede instalarse
export function canInstall() {
    // Verificar si ya está instalado
    if (window.matchMedia('(display-mode: standalone)').matches) {
        console.log('📱 PWA: Ya está instalada');
        return false;
    }

    // Verificar si está ejecutando en iOS
    const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
    if (isIos) {
        // En iOS Safari, siempre mostrar (no hay beforeinstallprompt)
        return true;
    }

    return isInstallable;
}

// Función para instalar la app
export async function installApp() {
    if (!deferredPrompt) {
        // Si no hay prompt (iOS o ya instalado)
        if (/iPad|iPhone|iPod/.test(navigator.userAgent)) {
            // Mostrar instrucciones para iOS
            if (window.Swal) {
                await window.Swal.fire({
                    title: 'Instalar en iOS',
                    html: `
                        <div style="text-align: left;">
                            <p>Para instalar esta app en tu iPhone o iPad:</p>
                            <ol style="padding-left: 20px;">
                                <li>Toca el botón <strong>Compartir</strong> 
                                    <svg style="display: inline; width: 16px; height: 16px; vertical-align: middle;" fill="currentColor" viewBox="0 0 16 16">
                                        <path d="M11 2.5a2.5 2.5 0 1 1 .603 1.628l-6.718 3.12a2.499 2.499 0 0 1 0 1.504l6.718 3.12a2.5 2.5 0 1 1-.488.876l-6.718-3.12a2.5 2.5 0 1 1 0-3.256l6.718-3.12A2.5 2.5 0 0 1 11 2.5z"/>
                                    </svg>
                                </li>
                                <li>Selecciona <strong>"Añadir a pantalla de inicio"</strong></li>
                                <li>Toca <strong>"Añadir"</strong></li>
                            </ol>
                        </div>
                    `,
                    icon: 'info',
                    confirmButtonText: 'Entendido'
                });
            }
            return false;
        }

        console.log('⚠️ PWA: No hay prompt disponible');
        return false;
    }

    try {
        // Mostrar el prompt de instalación
        deferredPrompt.prompt();

        // Esperar la respuesta del usuario
        const { outcome } = await deferredPrompt.userChoice;

        console.log(`📱 PWA: Usuario ${outcome === 'accepted' ? 'aceptó' : 'rechazó'} la instalación`);

        // Limpiar el prompt
        deferredPrompt = null;
        isInstallable = false;

        return outcome === 'accepted';
    } catch (error) {
        console.error('❌ PWA: Error al instalar:', error);
        return false;
    }
}

// Función para verificar si ya está instalada
export function isInstalled() {
    return window.matchMedia('(display-mode: standalone)').matches ||
        window.navigator.standalone === true;
}

// Auto-mostrar prompt después de cierto tiempo (opcional)
let autoPromptShown = false;
window.addEventListener('load', () => {
    // Esperar 30 segundos después de cargar
    setTimeout(() => {
        if (isInstallable && !autoPromptShown && !isInstalled()) {
            autoPromptShown = true;

            // Mostrar un banner sutil
            if (window.Toast) {
                window.Toast.fire({
                    icon: 'info',
                    title: '💡 Puedes instalar esta app en tu dispositivo',
                    timer: 5000,
                    showCloseButton: true
                });
            }
        }
    }, 30000); // 30 segundos
});

console.log('📱 PWA: Módulo de instalación cargado');
