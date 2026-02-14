using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace CarRental.Api
{
    /// <summary>
    /// Filtro para manejar correctamente IFormFile en Swagger
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Buscar parámetros IFormFile
            var fileParams = context.ApiDescription.ParameterDescriptions
                .Where(p => p.ModelMetadata != null &&
                           (p.ModelMetadata.ModelType == typeof(IFormFile) ||
                            p.ModelMetadata.ModelType == typeof(IFormFile[])))
                .ToList();

            if (!fileParams.Any())
                return;

            // Limpiar parámetros existentes que causan conflicto
            operation.Parameters?.Clear();

            // Crear el esquema para multipart/form-data
            var uploadFileSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new HashSet<string>()
            };

            foreach (var param in fileParams)
            {
                uploadFileSchema.Properties.Add(param.Name, new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = $"Archivo: {param.Name}"
                });

                if (param.IsRequired)
                {
                    uploadFileSchema.Required.Add(param.Name);
                }
            }

            // Configurar el RequestBody
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = uploadFileSchema
                    }
                }
            };
        }
    }
}