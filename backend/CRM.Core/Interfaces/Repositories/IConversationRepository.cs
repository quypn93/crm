using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IConversationRepository : IRepository<Conversation>
{
    /// <summary>
    /// Lấy tất cả conversation user đang là active participant, kèm participants + last message.
    /// </summary>
    Task<List<Conversation>> GetForUserAsync(Guid userId);

    /// <summary>
    /// Lấy 1 conversation kèm participants nếu user là participant active.
    /// </summary>
    Task<Conversation?> GetWithParticipantsAsync(Guid conversationId, Guid userId);

    /// <summary>
    /// Tìm direct conversation giữa 2 user (cả hai đều là active participant). Null nếu chưa có.
    /// </summary>
    Task<Conversation?> FindDirectAsync(Guid userA, Guid userB);
}
