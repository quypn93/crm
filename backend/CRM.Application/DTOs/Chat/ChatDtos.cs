using CRM.Core.Enums;

namespace CRM.Application.DTOs.Chat;

public class ConversationDto
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Name { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public Guid? LastMessageSenderId { get; set; }
    public int UnreadCount { get; set; }
    public List<ConversationParticipantDto> Participants { get; set; } = new();
}

public class ConversationParticipantDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime? LastReadAt { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDirectConversationDto
{
    public Guid OtherUserId { get; set; }
}

public class CreateGroupConversationDto
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> MemberUserIds { get; set; } = new();
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
}

public class AddParticipantsDto
{
    public List<Guid> UserIds { get; set; } = new();
}

public class RenameGroupDto
{
    public string Name { get; set; } = string.Empty;
}

public class MessageListFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}

public class ChatUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PrimaryRole { get; set; }
    public bool IsOnline { get; set; }
}
