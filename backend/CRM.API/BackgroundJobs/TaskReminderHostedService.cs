using CRM.Application.Interfaces;
using CRM.Application.Services;
using Microsoft.Extensions.Options;

namespace CRM.API.BackgroundJobs;

public class TaskReminderHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly NotificationOptions _options;
    private readonly ILogger<TaskReminderHostedService> _logger;

    public TaskReminderHostedService(
        IServiceProvider services,
        IOptions<NotificationOptions> options,
        ILogger<TaskReminderHostedService> logger)
    {
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = Math.Max(1, _options.JobIntervals.TaskReminderMinutes);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _logger.LogInformation("TaskReminderHostedService starting. Interval = {Interval} min", intervalMinutes);

        // Delay 30s đầu để tránh chạy job trước khi DB migration apply xong khi cold start.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = _services.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<ITaskReminderJob>();
                await job.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TaskReminderJob failed; will retry next tick.");
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
