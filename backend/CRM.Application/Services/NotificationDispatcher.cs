using AutoMapper;
using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationPreferenceService _preferences;
    private readonly IRealtimeNotifier _realtime;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationPreferenceService preferences,
        IRealtimeNotifier realtime,
        IEmailSender emailSender,
        ILogger<NotificationDispatcher> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _preferences = preferences;
        _realtime = realtime;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task DispatchAsync(NotificationEvent evt, CancellationToken ct = default)
    {
        await DispatchManyAsync(new[] { evt }, ct);
    }

    public async Task DispatchManyAsync(IEnumerable<NotificationEvent> events, CancellationToken ct = default)
    {
        var list = events.ToList();
        if (list.Count == 0) return;

        // Resolve preferences song song. ResolveForUserAsync chỉ đọc DbContext nên cần await tuần tự
        // (DbContext không thread-safe). Tối ưu hoá thực sự sẽ làm sau khi cần.
        var resolved = new List<(NotificationEvent Event, ResolvedPreference Pref)>();
        foreach (var evt in list)
        {
            var pref = await _preferences.ResolveForUserAsync(evt.RecipientUserId, evt.Type);
            resolved.Add((evt, pref));
        }

        // Step 1: insert in-app notifications batch
        var toInsert = new List<Notification>();
        foreach (var (evt, pref) in resolved)
        {
            if (!pref.InApp) continue;

            toInsert.Add(new Notification
            {
                RecipientUserId = evt.RecipientUserId,
                Type = evt.Type,
                Severity = evt.Severity,
                Title = evt.Title,
                Message = evt.Message,
                Link = evt.Link,
                EntityType = evt.EntityType,
                EntityId = evt.EntityId
            });
        }

        if (toInsert.Count > 0)
        {
            await _unitOfWork.Notifications.AddRangeAsync(toInsert);
            await _unitOfWork.SaveChangesAsync();

            // Step 2: push realtime cho mỗi recipient
            foreach (var notification in toInsert)
            {
                try
                {
                    var dto = _mapper.Map<NotificationDto>(notification);
                    await _realtime.NotifyUserAsync(notification.RecipientUserId, dto, ct);

                    var unread = await _unitOfWork.Notifications.GetUnreadCountAsync(notification.RecipientUserId);
                    await _realtime.NotifyUnreadCountAsync(notification.RecipientUserId, unread, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Realtime push thất bại cho user {UserId}", notification.RecipientUserId);
                }
            }
        }

        // Step 3: gửi email (best-effort, không throw)
        foreach (var (evt, pref) in resolved)
        {
            if (!pref.Email) continue;
            if (string.IsNullOrWhiteSpace(evt.EmailHtmlBody)) continue;

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(evt.RecipientUserId);
                if (user == null || !user.IsActive || string.IsNullOrWhiteSpace(user.Email)) continue;

                await _emailSender.SendAsync(
                    user.Email,
                    user.FullName,
                    evt.EmailSubject ?? evt.Title,
                    evt.EmailHtmlBody,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gửi email notification thất bại cho user {UserId}, type {Type}",
                    evt.RecipientUserId, evt.Type);
            }
        }
    }
}
