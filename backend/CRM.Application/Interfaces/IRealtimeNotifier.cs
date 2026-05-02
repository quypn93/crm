using CRM.Application.DTOs.Notification;

namespace CRM.Application.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);

    Task NotifyUnreadCountAsync(Guid userId, int unreadCount, CancellationToken ct = default);
}
