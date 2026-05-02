using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Notification> Items, int TotalCount)> GetPagedForUserAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize)
    {
        var query = _dbSet.Where(n => n.RecipientUserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> GetUnreadCountAsync(Guid userId)
    {
        return _dbSet.CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
    }

    public Task<Notification?> GetForUserAsync(Guid id, Guid userId)
    {
        return _dbSet.FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId);
    }

    public async Task<int> MarkAllReadAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.UpdatedAt, now));
    }

    public async Task<int> DeleteOldAsync(DateTime readCutoff, DateTime unreadCutoff)
    {
        return await _dbSet
            .Where(n => (n.IsRead && n.CreatedAt < readCutoff)
                     || (!n.IsRead && n.CreatedAt < unreadCutoff))
            .ExecuteDeleteAsync();
    }
}
