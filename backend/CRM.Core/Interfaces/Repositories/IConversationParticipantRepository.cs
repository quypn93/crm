using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IConversationParticipantRepository : IRepository<ConversationParticipant>
{
    Task<ConversationParticipant?> GetActiveAsync(Guid conversationId, Guid userId);

    Task<List<ConversationParticipant>> GetActiveByConversationAsync(Guid conversationId);

    Task<List<Guid>> GetActiveUserIdsAsync(Guid conversationId);
}
