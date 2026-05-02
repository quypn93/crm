namespace CRM.Application.Interfaces;

/// <summary>
/// Logic scan task DueDate để gửi notification TaskDueSoon / TaskOverdue.
/// Tách khỏi HostedService để dễ test + dễ migrate sang Hangfire/Quartz sau này.
/// </summary>
public interface ITaskReminderJob
{
    Task RunAsync(CancellationToken ct = default);
}
