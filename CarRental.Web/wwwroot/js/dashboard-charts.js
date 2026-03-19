// dashboard-charts.js - Gráficos para Dashboard con Chart.js
// VERSIÓN CON DEBUG

console.log('[Dashboard Charts] Script cargado');

// Verificar que Chart.js está disponible
if (typeof Chart === 'undefined') {
    console.error('[Dashboard Charts] ERROR: Chart.js NO está cargado!');
} else {
    console.log('[Dashboard Charts] Chart.js versión:', Chart.version);
}

window.createDoughnutChart = function (canvasId, data) {
    console.log('[createDoughnutChart] Iniciando...');
    console.log('[createDoughnutChart] canvasId:', canvasId);
    console.log('[createDoughnutChart] data:', data);

    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error(`[createDoughnutChart] ERROR: Canvas ${canvasId} NO encontrado`);
        return;
    }
    console.log('[createDoughnutChart] Canvas encontrado:', ctx);

    // Verificar que Chart está disponible
    if (typeof Chart === 'undefined') {
        console.error('[createDoughnutChart] ERROR: Chart.js no está disponible');
        return;
    }

    // Destruir gráfico anterior si existe
    const existingChart = Chart.getChart(canvasId);
    if (existingChart) {
        console.log('[createDoughnutChart] Destruyendo gráfico anterior');
        existingChart.destroy();
    }

    try {
        console.log('[createDoughnutChart] Creando nuevo gráfico...');
        const chart = new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        titleFont: {
                            size: 14,
                            weight: 'bold'
                        },
                        bodyFont: {
                            size: 13
                        },
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '70%'
            }
        });
        console.log('[createDoughnutChart] ✅ Gráfico creado exitosamente:', chart);
    } catch (error) {
        console.error('[createDoughnutChart] ❌ ERROR al crear gráfico:', error);
    }
};

window.createBarChart = function (canvasId, data) {
    console.log('[createBarChart] Iniciando...');
    console.log('[createBarChart] canvasId:', canvasId);
    console.log('[createBarChart] data:', data);

    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error(`[createBarChart] ERROR: Canvas ${canvasId} NO encontrado`);
        return;
    }
    console.log('[createBarChart] Canvas encontrado:', ctx);

    // Verificar que Chart está disponible
    if (typeof Chart === 'undefined') {
        console.error('[createBarChart] ERROR: Chart.js no está disponible');
        return;
    }

    // Destruir gráfico anterior si existe
    const existingChart = Chart.getChart(canvasId);
    if (existingChart) {
        console.log('[createBarChart] Destruyendo gráfico anterior');
        existingChart.destroy();
    }

    try {
        console.log('[createBarChart] Creando nuevo gráfico...');
        const chart = new Chart(ctx, {
            type: 'bar',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                indexAxis: 'y',
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            boxWidth: 12,
                            padding: 15,
                            font: {
                                size: 12,
                                weight: 'bold'
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        titleFont: {
                            size: 14,
                            weight: 'bold'
                        },
                        bodyFont: {
                            size: 13
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            font: {
                                size: 11
                            }
                        },
                        grid: {
                            display: true,
                            color: 'rgba(0, 0, 0, 0.05)'
                        }
                    },
                    y: {
                        ticks: {
                            font: {
                                size: 12,
                                weight: 'bold'
                            }
                        },
                        grid: {
                            display: false
                        }
                    }
                },
                barThickness: 30
            }
        });
        console.log('[createBarChart] ✅ Gráfico creado exitosamente:', chart);
    } catch (error) {
        console.error('[createBarChart] ❌ ERROR al crear gráfico:', error);
    }
};

console.log('[Dashboard Charts] Funciones registradas en window:', {
    createDoughnutChart: typeof window.createDoughnutChart,
    createBarChart: typeof window.createBarChart
});
