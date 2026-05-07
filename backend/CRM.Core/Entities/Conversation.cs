using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class Conversation : BaseEntity
{
    public ConversationType Type { get; set; }

    public string? Name { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
