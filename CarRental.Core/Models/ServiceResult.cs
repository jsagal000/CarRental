// CarRental.Core/Models/ServiceResult.cs
using System;
using System.Collections.Generic;

namespace CarRental.Core.Models
{
    /// <summary>
    /// Represents the result of an operation, indicating success or failure,
    /// and optionally carrying data and an error message.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the data returned by the operation if successful.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Private constructor to enforce creation via static factory methods.
        /// </summary>
        private ServiceResult(bool isSuccess, T data, string errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful <see cref="ServiceResult{T}"/> with data.
        /// </summary>
        /// <param name="data">The data returned by the successful operation.</param>
        /// <returns>A successful <see cref="ServiceResult{T}"/> instance.</returns>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>(true, data, null);
        }

        /// <summary>
        /// Creates a failed <see cref="ServiceResult{T}"/> with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <returns>A failed <see cref="ServiceResult{T}"/> instance.</returns>
        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>(false, default(T), errorMessage);
        }
    }

    /// <summary>
    /// Non-generic version of ServiceResult for operations that do not return data.
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        private ServiceResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult Success()
        {
            return new ServiceResult(true, null);
        }

        public static ServiceResult Failure(string errorMessage)
        {
            return new ServiceResult(false, errorMessage);
        }
    }
}
