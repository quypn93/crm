using CRM.API.Hubs;
using CRM.Application.DTOs.Chat;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Realtime;

public class SignalRChatNotifier : IChatRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _hub;

    public SignalRChatNotifier(IHubContext<ChatHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyMessageAsync(IEnumerable<Guid> userIds, ChatMessageDto message, CancellationToken ct = default)
    {
        var groups = userIds.Select(ChatHub.GroupName).ToList();
        if (groups.Count == 0) return Task.CompletedTask;
        return _hub.Clients.Groups(groups).SendAsync("chatMessage", message, ct);
    }

    public Task NotifyConversationUpsertAsync(IEnumerable<Guid> userIds, ConversationDto conversation, CancellationToken ct = default)
    {
        var groups = userIds.Select(ChatHub.GroupName).ToList();
        if (groups.Count == 0) return Task.CompletedTask;
        return _hub.Clients.Groups(groups).SendAsync("chatConversation", conversation, ct);
    }

    public Task NotifyTotalUnreadAsync(Guid userId, int totalUnread, CancellationToken ct = default)
    {
        return _hub.Clients
            .Group(ChatHub.GroupName(userId))
            .SendAsync("chatUnreadCount", totalUnread, ct);
    }
}
