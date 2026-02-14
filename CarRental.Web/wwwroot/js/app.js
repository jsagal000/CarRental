// app.js

window.appFunctions = {
    // Función para dibujar un gráfico de barras usando Plotly.js
    drawBarChart: function (elementId, labels, values, chartTitle, xTitle, yTitle) {
        const data = [{
            x: labels,
            y: values,
            type: 'bar',
            marker: {
                color: 'rgba(54, 162, 235, 0.8)' // Color de las barras
            }
        }];

        const layout = {
            title: chartTitle,
            xaxis: {
                title: xTitle,
                automargin: true // Ajusta automáticamente los márgenes del eje x
            },
            yaxis: {
                title: yTitle,
                tickprefix: '$', // Añade el símbolo de dólar a las etiquetas del eje y
                automargin: true
            },
            margin: {
                l: 60, // Margen izquierdo para etiquetas del eje y
                r: 20, // Margen derecho
                b: 100, // Margen inferior para etiquetas del eje x
                t: 60, // Margen superior para el título
                pad: 4 // Relleno
            },
            responsive: true // Hace que el gráfico sea responsivo
        };

        Plotly.newPlot(elementId, data, layout, { responsive: true });
    },

    // Función para dibujar un gráfico de líneas usando Plotly.js
    drawLineChart: function (elementId, labels, values, chartTitle, xTitle, yTitle) {
        const data = [{
            x: labels,
            y: values,
            mode: 'lines+markers',
            type: 'scatter',
            line: {
                color: 'rgba(75, 192, 192, 1)', // Color de la línea
                width: 3
            },
            marker: {
                color: 'rgba(75, 192, 192, 1)',
                size: 8
            }
        }];

        const layout = {
            title: chartTitle,
            xaxis: {
                title: xTitle,
                automargin: true
            },
            yaxis: {
                title: yTitle,
                tickprefix: '$', // Añade el símbolo de dólar a las etiquetas del eje y
                automargin: true
            },
            margin: {
                l: 60,
                r: 20,
                b: 100,
                t: 60,
                pad: 4
            },
            responsive: true
        };

        Plotly.newPlot(elementId, data, layout, { responsive: true });
    },

    // Función para exportar a PDF usando jsPDF
    exportToPdf: function (title, dateRange, contentLines) {
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF();

        let yOffset = 20;
        const lineHeight = 10;
        const margin = 20;
        const maxWidth = doc.internal.pageSize.getWidth() - 2 * margin;

        doc.setFontSize(18);
        doc.text(title, doc.internal.pageSize.getWidth() / 2, yOffset, { align: 'center' });
        yOffset += lineHeight * 2;

        doc.setFontSize(12);
        doc.text(dateRange, margin, yOffset);
        yOffset += lineHeight * 2;

        doc.setFontSize(10);
        contentLines.forEach(line => {
            const lines = doc.splitTextToSize(line, maxWidth);
            doc.text(lines, margin, yOffset);
            yOffset += lineHeight * lines.length;
            if (yOffset > doc.internal.pageSize.getHeight() - margin) {
                doc.addPage();
                yOffset = margin;
            }
        });

        doc.save("ReporteFinanciero.pdf");
    }
};
