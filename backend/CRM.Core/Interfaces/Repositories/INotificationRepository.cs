using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<(IEnumerable<Notification> Items, int TotalCount)> GetPagedForUserAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task<Notification?> GetForUserAsync(Guid id, Guid userId);

    Task<int> MarkAllReadAsync(Guid userId);

    Task<int> DeleteOldAsync(DateTime readCutoff, DateTime unreadCutoff);
}
