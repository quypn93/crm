using CRM.API.Hubs;
using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Realtime;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
    {
        return _hub.Clients
            .Group(NotificationHub.GroupName(userId))
            .SendAsync("notification", notification, ct);
    }

    public Task NotifyUnreadCountAsync(Guid userId, int unreadCount, CancellationToken ct = default)
    {
        return _hub.Clients
            .Group(NotificationHub.GroupName(userId))
            .SendAsync("unreadCount", unreadCount, ct);
    }
}
