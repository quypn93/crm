using CRM.Application.Interfaces;
using CRM.Application.Services;
using Microsoft.Extensions.Options;

namespace CRM.API.BackgroundJobs;

public class NotificationCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationCleanupHostedService> _logger;

    public NotificationCleanupHostedService(
        IServiceProvider services,
        IOptions<NotificationOptions> options,
        ILogger<NotificationCleanupHostedService> logger)
    {
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalHours = Math.Max(1, _options.JobIntervals.CleanupHours);
        var interval = TimeSpan.FromHours(intervalHours);

        _logger.LogInformation("NotificationCleanupHostedService starting. Interval = {Interval}h", intervalHours);

        // Delay 5 phút sau khi app start để tránh chạy job ngay lúc warm-up.
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = _services.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<INotificationCleanupJob>();
                await job.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationCleanupJob failed; will retry next tick.");
            }
        }
        while (await SafeWaitAsync(timer, stoppingToken));
    }

    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }
}
