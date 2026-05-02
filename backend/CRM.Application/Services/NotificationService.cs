using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRealtimeNotifier _realtime;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, IRealtimeNotifier realtime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _realtime = realtime;
    }

    public async Task<PaginatedResult<NotificationDto>> GetForUserAsync(Guid userId, NotificationFilterDto filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, total) = await _unitOfWork.Notifications.GetPagedForUserAsync(userId, filter.UnreadOnly, page, pageSize);
        var dtos = _mapper.Map<List<NotificationDto>>(items);
        return PaginatedResult<NotificationDto>.Create(dtos, total, page, pageSize);
    }

    public Task<int> GetUnreadCountAsync(Guid userId)
    {
        return _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task<bool> MarkReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetForUserAsync(notificationId, userId);
        if (notification == null) return false;
        if (notification.IsRead) return true;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        var unread = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        await _realtime.NotifyUnreadCountAsync(userId, unread);
        return true;
    }

    public async Task<int> MarkAllReadAsync(Guid userId)
    {
        var count = await _unitOfWork.Notifications.MarkAllReadAsync(userId);
        if (count > 0)
        {
            await _realtime.NotifyUnreadCountAsync(userId, 0);
        }
        return count;
    }

    public async Task<bool> DeleteAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetForUserAsync(notificationId, userId);
        if (notification == null) return false;

        var wasUnread = !notification.IsRead;
        _unitOfWork.Notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync();

        if (wasUnread)
        {
            var unread = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
            await _realtime.NotifyUnreadCountAsync(userId, unread);
        }
        return true;
    }
}
