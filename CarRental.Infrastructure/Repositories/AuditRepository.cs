using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IDbContextFactory<CarRentalDbContext> _contextFactory;

        public AuditRepository(IDbContextFactory<CarRentalDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                await context.AuditLogs.AddAsync(auditLog);
                await context.SaveChangesAsync();

                Console.WriteLine($"[AUDIT REPO] Log guardado ID: {auditLog.Id}");

                return auditLog;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDIT REPO ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogFilterDto filter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Cargar sin Include primero para ver si ese es el problema
            var query = context.AuditLogs.AsQueryable();

            // Aplicar filtros
            if (filter.StartDate.HasValue)
                query = query.Where(al => al.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(al => al.CreatedAt <= filter.EndDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(filter.Module))
                query = query.Where(al => al.Module == filter.Module);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(al => al.Action == filter.Action);

            if (filter.UserId.HasValue)
                query = query.Where(al => al.UserId == filter.UserId.Value);

            if (filter.IsSuccess.HasValue)
                query = query.Where(al => al.IsSuccess == filter.IsSuccess.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(al => al.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Cargar usuarios en una segunda consulta para evitar problemas
            var userIds = items.Select(al => al.UserId).Distinct().ToList();
            var users = await context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            // Asignar usuarios manualmente
            foreach (var item in items)
            {
                if (users.ContainsKey(item.UserId))
                {
                    item.User = users[item.UserId];
                }
            }

            // Filtro por UserName después de cargar (si se especificó)
            if (!string.IsNullOrEmpty(filter.UserName))
            {
                items = items.Where(al =>
                    al.User != null &&
                    al.User.Username.Contains(filter.UserName, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return new PagedResult<AuditLog>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.AuditLogs
                .Where(al => al.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(al => al.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(al => al.CreatedAt <= endDate.Value.AddDays(1));

            var items = await query
                .AsNoTracking()
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();

            // Cargar usuario
            var user = await context.Users.FindAsync(userId);
            foreach (var item in items)
            {
                item.User = user;
            }

            return items;
        }

        public async Task<Dictionary<string, int>> GetActivitySummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(al => al.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(al => al.CreatedAt <= endDate.Value.AddDays(1));

            return await query
                .GroupBy(al => new { al.Module, al.Action })
                .Select(g => new { Key = g.Key.Module + "." + g.Key.Action, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }
    }
}