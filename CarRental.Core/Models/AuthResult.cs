namespace CarRental.Core.Models
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public static AuthResult Success(string token, User user, DateTime expiresAt)
        {
            return new AuthResult
            {
                IsSuccess = true,
                Token = token,
                User = user,
                ExpiresAt = expiresAt
            };
        }

        public static AuthResult Failure(string errorMessage)
        {
            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}