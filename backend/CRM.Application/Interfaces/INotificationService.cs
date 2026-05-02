using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Notification;

namespace CRM.Application.Interfaces;

public interface INotificationService
{
    Task<PaginatedResult<NotificationDto>> GetForUserAsync(Guid userId, NotificationFilterDto filter);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task<bool> MarkReadAsync(Guid notificationId, Guid userId);

    Task<int> MarkAllReadAsync(Guid userId);

    Task<bool> DeleteAsync(Guid notificationId, Guid userId);
}
