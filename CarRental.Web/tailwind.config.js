/** @type {import('tailwindcss').Config} */
module.exports = {
  // Configuración de los archivos que Tailwind debe escanear para encontrar clases.
  // Es crucial que estas rutas sean correctas para que Tailwind genere el CSS.
  content: [
    './**/*.{razor,html,cshtml}', // Escanea todos los archivos .razor, .html y .cshtml en todas las subcarpetas
    './_Imports.razor',         // Incluye _Imports.razor si defines clases allí
    './Pages/**/*.{razor,html,cshtml}', // Específico para componentes en la carpeta Pages
    './Shared/**/*.{razor,html,cshtml}', // Específico para componentes en la carpeta Shared
    './wwwroot/index.html',     // Para la página principal de Blazor WebAssembly
    ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'], // Define la fuente Inter como predeterminada
      },
    },
  },
  plugins: [],
}
