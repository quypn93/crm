using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<(IEnumerable<ChatMessage> Items, int TotalCount)> GetPagedAsync(
        Guid conversationId,
        int page,
        int pageSize);

    /// <summary>
    /// Đếm message chưa đọc cho user trong 1 conversation (newer than LastReadAt).
    /// </summary>
    Task<int> CountUnreadAsync(Guid conversationId, Guid userId, DateTime? lastReadAt);

    /// <summary>
    /// Tổng số message chưa đọc của user trên toàn bộ conversation đang active.
    /// </summary>
    Task<int> CountTotalUnreadAsync(Guid userId);
}
