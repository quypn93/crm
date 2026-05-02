namespace CRM.Application.Services;

public class NotificationOptions
{
    public int RetentionReadDays { get; set; } = 30;
    public int RetentionUnreadDays { get; set; } = 90;

    /// <summary>
    /// Default per-role config khi chưa có row trong DB.
    /// Key 1 = role name (vd "SalesManager"), Key 2 = NotificationType name (vd "TaskAssigned").
    /// Giá trị có thể là "_FALLBACK_ALL" để áp dụng cho mọi type chưa khai báo.
    /// </summary>
    public Dictionary<string, Dictionary<string, ChannelConfig>> RoleDefaults { get; set; } = new();

    public JobIntervalsConfig JobIntervals { get; set; } = new();
}

public class ChannelConfig
{
    public bool InApp { get; set; } = true;
    public bool Email { get; set; }
}

public class JobIntervalsConfig
{
    public int CleanupHours { get; set; } = 6;
    public int TaskReminderMinutes { get; set; } = 30;
}
