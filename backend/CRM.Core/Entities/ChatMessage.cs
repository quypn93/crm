namespace CRM.Core.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User SenderUser { get; set; } = null!;
}
