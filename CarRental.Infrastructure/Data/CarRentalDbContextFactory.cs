// CarRental.Infrastructure/Data/CarRentalDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO; // Necesario para Directory.GetCurrentDirectory()

namespace CarRental.Infrastructure.Data
{
    // Esta clase le dice a las herramientas de Entity Framework Core cómo crear una instancia
    // de CarRentalDbContext cuando se ejecutan comandos como Add-Migration o Update-Database.
    public class CarRentalDbContextFactory : IDesignTimeDbContextFactory<CarRentalDbContext>
    {
        public CarRentalDbContext CreateDbContext(string[] args)
        {
            // Configura las opciones para el DbContext.
            var optionsBuilder = new DbContextOptionsBuilder<CarRentalDbContext>();

            // Aquí, configuramos SQLite. Es importante que la cadena de conexión
            // refleje cómo tu aplicación principal (CarRental.Api) espera la DB.
            // En tiempo de diseño, a menudo se usa una ruta relativa al directorio de ejecución.
            // Obtenemos la ruta del directorio actual donde se ejecutarán las herramientas de migración.
            // Esto usualmente apuntará a la carpeta del proyecto de inicio (CarRental.Api/bin/Debug/net8.0)
            // si la base de datos no está en el root del proyecto.
            // Para SQLite, el archivo CarRental.db se creará en el directorio de ejecución del startup project.
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "CarRental.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            // Si estuvieras usando SQL Server y quisieras leer de appsettings.json,
            // la lógica sería más compleja, involucrando ConfigurationBuilder.
            // Pero para SQLite y simplificar el diseño, esto es suficiente.

            return new CarRentalDbContext(optionsBuilder.Options);
        }
    }
}