using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ConversationParticipantRepository : Repository<ConversationParticipant>, IConversationParticipantRepository
{
    public ConversationParticipantRepository(CrmDbContext context) : base(context)
    {
    }

    public Task<ConversationParticipant?> GetActiveAsync(Guid conversationId, Guid userId)
    {
        return _dbSet.FirstOrDefaultAsync(p =>
            p.ConversationId == conversationId
            && p.UserId == userId
            && p.IsActive);
    }

    public Task<List<ConversationParticipant>> GetActiveByConversationAsync(Guid conversationId)
    {
        return _dbSet
            .Where(p => p.ConversationId == conversationId && p.IsActive)
            .Include(p => p.User)
            .ToListAsync();
    }

    public Task<List<Guid>> GetActiveUserIdsAsync(Guid conversationId)
    {
        return _dbSet
            .Where(p => p.ConversationId == conversationId && p.IsActive)
            .Select(p => p.UserId)
            .ToListAsync();
    }
}
