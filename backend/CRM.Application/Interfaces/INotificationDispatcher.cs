using CRM.Core.Enums;

namespace CRM.Application.Interfaces;

public class NotificationEvent
{
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }

    // Email-specific (chỉ dùng khi event được preference cho phép gửi email)
    public string? EmailSubject { get; set; }
    public string? EmailHtmlBody { get; set; }
}

public interface INotificationDispatcher
{
    Task DispatchAsync(NotificationEvent evt, CancellationToken ct = default);

    Task DispatchManyAsync(IEnumerable<NotificationEvent> events, CancellationToken ct = default);
}
