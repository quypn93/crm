using CRM.Application.DTOs.Chat;
using CRM.Application.DTOs.Common;

namespace CRM.Application.Interfaces;

public interface IChatService
{
    Task<List<ConversationDto>> GetMyConversationsAsync(Guid userId);

    Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid userId);

    Task<ConversationDto> CreateOrGetDirectAsync(Guid currentUserId, CreateDirectConversationDto dto);

    Task<ConversationDto> CreateGroupAsync(Guid currentUserId, CreateGroupConversationDto dto);

    Task<ConversationDto> RenameGroupAsync(Guid conversationId, Guid currentUserId, RenameGroupDto dto);

    Task<ConversationDto> AddParticipantsAsync(Guid conversationId, Guid currentUserId, AddParticipantsDto dto);

    Task<bool> LeaveConversationAsync(Guid conversationId, Guid currentUserId);

    Task<PaginatedResult<ChatMessageDto>> GetMessagesAsync(Guid conversationId, Guid currentUserId, MessageListFilterDto filter);

    Task<ChatMessageDto> SendMessageAsync(Guid conversationId, Guid currentUserId, SendMessageDto dto);

    Task<int> MarkReadAsync(Guid conversationId, Guid currentUserId);

    Task<int> GetTotalUnreadAsync(Guid userId);

    /// <summary>Lấy danh sách user khả dụng cho chat (active, khác current user). IsOnline luôn = false ở layer này — controller sẽ overlay presence.</summary>
    Task<List<ChatUserDto>> GetChatUsersAsync(Guid currentUserId);
}
