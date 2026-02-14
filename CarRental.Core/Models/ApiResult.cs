// CarRental.Core/Models/ApiResult.cs
using System.Text.Json.Serialization;

namespace CarRental.Core.Models
{
    // Clase base no genérica para resultados de API sin datos de retorno
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        // Constructor sin parámetros requerido para deserialización JSON
        public ApiResult()
        {
        }

        // Constructor PROTECTED para permitir que las clases derivadas accedan a él
        protected ApiResult(bool isSuccess, string errorMessage, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        // Método estático para un resultado de éxito sin mensaje
        public static ApiResult Success()
        {
            return new ApiResult(true, null, 200);
        }

        // Método estático para un resultado de éxito con un mensaje opcional
        public static ApiResult Success(string message)
        {
            return new ApiResult(true, message, 200);
        }

        // Método estático para un resultado de fallo con un mensaje de error
        public static ApiResult Failure(string errorMessage)
        {
            return new ApiResult(false, errorMessage, 500);
        }

        // Sobrecarga para un resultado de fallo con un mensaje de error y un código de estado
        public static ApiResult Failure(string errorMessage, int statusCode)
        {
            return new ApiResult(false, errorMessage, statusCode);
        }
    }

    // Clase genérica para resultados de API con datos de retorno
    public class ApiResult<T> : ApiResult
    {
        public T Data { get; set; }

        // Constructor sin parámetros requerido para deserialización JSON
        public ApiResult()
        {
        }

        // Constructor con JsonConstructor para deserialización específica
        [JsonConstructor]
        public ApiResult(bool isSuccess, T data, string errorMessage, int statusCode)
            : base(isSuccess, errorMessage, statusCode)
        {
            Data = data;
        }

        // Constructor privado para uso interno (mantener compatibilidad)
        private ApiResult(bool isSuccess, T data, string errorMessage, int statusCode, bool usePrivate)
            : base(isSuccess, errorMessage, statusCode)
        {
            Data = data;
        }

        // Método estático para un resultado de éxito con datos
        public static new ApiResult<T> Success(T data, int statusCode = 200)
        {
            return new ApiResult<T>(true, data, null, statusCode, true);
        }

        // Método estático para un resultado de éxito con datos y un mensaje opcional
        public static new ApiResult<T> Success(T data, string message, int statusCode = 200)
        {
            return new ApiResult<T>(true, data, message, statusCode, true);
        }

        // Método estático para un resultado de fallo con un mensaje de error (sin datos)
        public static new ApiResult<T> Failure(string errorMessage, int statusCode = 500)
        {
            return new ApiResult<T>(false, default(T), errorMessage, statusCode, true);
        }
    }
}