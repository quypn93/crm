using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<ChatMessage> Items, int TotalCount)> GetPagedAsync(
        Guid conversationId,
        int page,
        int pageSize)
    {
        var query = _dbSet.Where(m => m.ConversationId == conversationId);

        var total = await query.CountAsync();

        // Page=1 lấy message mới nhất → desc rồi reverse ở UI để hiển thị từ cũ → mới.
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.SenderUser)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> CountUnreadAsync(Guid conversationId, Guid userId, DateTime? lastReadAt)
    {
        var query = _dbSet
            .Where(m => m.ConversationId == conversationId && m.SenderUserId != userId);

        if (lastReadAt.HasValue)
        {
            query = query.Where(m => m.CreatedAt > lastReadAt.Value);
        }

        return query.CountAsync();
    }

    public Task<int> CountTotalUnreadAsync(Guid userId)
    {
        // Đếm tổng số message từ tất cả conversation user là active participant
        // mà message tạo sau LastReadAt của user đó (hoặc toàn bộ nếu chưa đọc lần nào).
        return _context.ConversationParticipants
            .Where(p => p.UserId == userId && p.IsActive)
            .SelectMany(p => _context.ChatMessages
                .Where(m => m.ConversationId == p.ConversationId
                    && m.SenderUserId != userId
                    && (p.LastReadAt == null || m.CreatedAt > p.LastReadAt)))
            .CountAsync();
    }
}
