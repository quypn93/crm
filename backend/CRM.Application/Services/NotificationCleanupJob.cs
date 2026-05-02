using CRM.Application.Interfaces;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Application.Services;

public class NotificationCleanupJob : INotificationCleanupJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationCleanupJob> _logger;

    public NotificationCleanupJob(
        IUnitOfWork unitOfWork,
        IOptions<NotificationOptions> options,
        ILogger<NotificationCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var readCutoff = now.AddDays(-_options.RetentionReadDays);
        var unreadCutoff = now.AddDays(-_options.RetentionUnreadDays);

        var deleted = await _unitOfWork.Notifications.DeleteOldAsync(readCutoff, unreadCutoff);
        if (deleted > 0)
        {
            _logger.LogInformation(
                "NotificationCleanupJob: deleted {Count} notifications " +
                "(read cutoff {ReadCutoff:o}, unread cutoff {UnreadCutoff:o})",
                deleted, readCutoff, unreadCutoff);
        }
    }
}
