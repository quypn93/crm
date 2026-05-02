using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Services;

public class TaskReminderJob : ITaskReminderJob
{
    private const int DueSoonHorizonHours = 24;

    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<TaskReminderJob> _logger;

    public TaskReminderJob(
        IUnitOfWork unitOfWork,
        INotificationDispatcher dispatcher,
        ILogger<TaskReminderJob> logger)
    {
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var horizon = now.AddHours(DueSoonHorizonHours);

        // Step 1: Due soon (DueDate trong 24h tới)
        var dueSoonTasks = (await _unitOfWork.Tasks
            .GetDueSoonNotNotifiedAsync(now, horizon, NotificationType.TaskDueSoon)).ToList();

        // Step 2: Overdue
        var overdueTasks = (await _unitOfWork.Tasks
            .GetOverdueNotNotifiedAsync(now, NotificationType.TaskOverdue)).ToList();

        if (dueSoonTasks.Count == 0 && overdueTasks.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "TaskReminderJob: {DueSoon} due-soon + {Overdue} overdue tasks need notification",
            dueSoonTasks.Count, overdueTasks.Count);

        // Build events + log entries
        var events = new List<NotificationEvent>();
        var logs = new List<TaskNotificationLog>();

        foreach (var task in dueSoonTasks)
        {
            events.Add(BuildDueSoonEvent(task));
            logs.Add(new TaskNotificationLog
            {
                TaskId = task.Id,
                Type = NotificationType.TaskDueSoon,
                SentAt = now
            });
        }

        foreach (var task in overdueTasks)
        {
            events.Add(BuildOverdueEvent(task));
            logs.Add(new TaskNotificationLog
            {
                TaskId = task.Id,
                Type = NotificationType.TaskOverdue,
                SentAt = now
            });
        }

        // Log đánh dấu trước khi dispatch — phòng case dispatcher fail thì lần sau không spam lại.
        // Trade-off: nếu dispatcher fail user sẽ bị miss notification, nhưng đó là acceptable vì
        // alternative (gửi trùng nhiều lần) tệ hơn. Nếu fail nhiều thì admin xoá log rồi job tự pick lại.
        await _unitOfWork.TaskNotificationLogs.AddRangeAsync(logs);
        await _unitOfWork.SaveChangesAsync();

        await _dispatcher.DispatchManyAsync(events, ct);
    }

    private static NotificationEvent BuildDueSoonEvent(TaskItem task)
    {
        var dueLocal = task.DueDate!.Value.ToLocalTime();
        return new NotificationEvent
        {
            Type = NotificationType.TaskDueSoon,
            Severity = NotificationSeverity.Warning,
            RecipientUserId = task.AssignedToUserId!.Value,
            Title = "Công việc sắp đến hạn",
            Message = $"{task.Title} — đến hạn lúc {dueLocal:dd/MM/yyyy HH:mm}",
            Link = $"/tasks/{task.Id}/edit",
            EntityType = "Task",
            EntityId = task.Id,
            EmailSubject = $"[CRM] Công việc sắp đến hạn: {task.Title}",
            EmailHtmlBody = BuildEmailHtml(
                "Công việc sắp đến hạn",
                $"<p><strong>{task.Title}</strong></p>"
                + (string.IsNullOrEmpty(task.Description) ? string.Empty : $"<p>{task.Description}</p>")
                + $"<p>Đến hạn lúc: <strong>{dueLocal:dd/MM/yyyy HH:mm}</strong></p>",
                $"/tasks/{task.Id}/edit",
                "Mở công việc")
        };
    }

    private static NotificationEvent BuildOverdueEvent(TaskItem task)
    {
        var dueLocal = task.DueDate!.Value.ToLocalTime();
        return new NotificationEvent
        {
            Type = NotificationType.TaskOverdue,
            Severity = NotificationSeverity.Error,
            RecipientUserId = task.AssignedToUserId!.Value,
            Title = "Công việc đã quá hạn",
            Message = $"{task.Title} — quá hạn từ {dueLocal:dd/MM/yyyy HH:mm}",
            Link = $"/tasks/{task.Id}/edit",
            EntityType = "Task",
            EntityId = task.Id,
            EmailSubject = $"[CRM] CÔNG VIỆC QUÁ HẠN: {task.Title}",
            EmailHtmlBody = BuildEmailHtml(
                "Công việc đã quá hạn",
                $"<p><strong>{task.Title}</strong></p>"
                + (string.IsNullOrEmpty(task.Description) ? string.Empty : $"<p>{task.Description}</p>")
                + $"<p style=\"color:#dc2626;\">Quá hạn từ: <strong>{dueLocal:dd/MM/yyyy HH:mm}</strong></p>",
                $"/tasks/{task.Id}/edit",
                "Xử lý ngay")
        };
    }

    private static string BuildEmailHtml(string heading, string bodyHtml, string relativeLink, string ctaText)
    {
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;"">
  <h2 style=""color: #2563eb;"">{heading}</h2>
  {bodyHtml}
  <p style=""margin-top: 24px;"">
    <a href=""{relativeLink}"" style=""display:inline-block;padding:10px 20px;background:#2563eb;color:#fff;text-decoration:none;border-radius:6px;"">{ctaText}</a>
  </p>
  <hr style=""margin-top:32px;border:none;border-top:1px solid #e5e7eb;"">
  <p style=""color: #6b7280; font-size: 12px;"">CRM Đồng Phục Bốn Mùa — vui lòng không trả lời email này.</p>
</body>
</html>";
    }
}
