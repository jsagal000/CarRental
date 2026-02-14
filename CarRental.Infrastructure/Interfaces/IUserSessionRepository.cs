using CarRental.Core.Models;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IUserSessionRepository : IGenericRepository<UserSession>
    {
        Task<UserSession> GetByTokenAsync(string token);
        Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(int userId);
        Task RevokeSessionAsync(string token);
        Task RevokeAllUserSessionsAsync(int userId);
        Task CleanExpiredSessionsAsync();
    }
}