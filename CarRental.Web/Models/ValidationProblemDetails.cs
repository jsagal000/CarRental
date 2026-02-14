using System.Collections.Generic;

namespace CarRental.Web.Models
{
    // Esta clase coincide con el formato de error de validación estándar de ASP.NET Core
    public class ValidationProblemDetails
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}