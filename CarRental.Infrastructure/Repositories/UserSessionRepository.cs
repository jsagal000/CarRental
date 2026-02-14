using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class UserSessionRepository : GenericRepository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<UserSession> GetByTokenAsync(string token)
        {
            return await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Token == token && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(int userId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeSessionAsync(string token)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Token == token);

            if (session != null)
            {
                session.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserSessionsAsync(int userId)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanExpiredSessionsAsync()
        {
            var expiredSessions = await _context.UserSessions
                .Where(s => s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.UserSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
        }
    }
}