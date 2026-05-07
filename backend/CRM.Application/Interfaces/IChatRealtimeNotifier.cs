using CRM.Application.DTOs.Chat;

namespace CRM.Application.Interfaces;

/// <summary>
/// Realtime push cho chat — broadcast message tới tất cả participants của conversation
/// và update unread counter cho từng user.
/// </summary>
public interface IChatRealtimeNotifier
{
    Task NotifyMessageAsync(IEnumerable<Guid> userIds, ChatMessageDto message, CancellationToken ct = default);

    Task NotifyConversationUpsertAsync(IEnumerable<Guid> userIds, ConversationDto conversation, CancellationToken ct = default);

    Task NotifyTotalUnreadAsync(Guid userId, int totalUnread, CancellationToken ct = default);
}
