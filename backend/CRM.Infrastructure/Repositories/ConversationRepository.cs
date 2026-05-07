using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<List<Conversation>> GetForUserAsync(Guid userId)
    {
        // Lấy conversation user đang active. Sort theo LastMessageAt desc, fallback CreatedAt.
        return await _dbSet
            .Where(c => c.Participants.Any(p => p.UserId == userId && p.IsActive))
            .Include(c => c.Participants.Where(p => p.IsActive))
                .ThenInclude(p => p.User)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetWithParticipantsAsync(Guid conversationId, Guid userId)
    {
        return await _dbSet
            .Where(c => c.Id == conversationId
                && c.Participants.Any(p => p.UserId == userId && p.IsActive))
            .Include(c => c.Participants.Where(p => p.IsActive))
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync();
    }

    public async Task<Conversation?> FindDirectAsync(Guid userA, Guid userB)
    {
        // Direct conversation giữa 2 user: type=Direct, có đúng 2 participant active = {userA, userB}.
        return await _dbSet
            .Where(c => c.Type == ConversationType.Direct
                && c.Participants.Count(p => p.IsActive) == 2
                && c.Participants.Any(p => p.UserId == userA && p.IsActive)
                && c.Participants.Any(p => p.UserId == userB && p.IsActive))
            .Include(c => c.Participants.Where(p => p.IsActive))
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync();
    }
}
