namespace CRM.Core.Entities;

public class ConversationParticipant : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public DateTime? LastReadAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
