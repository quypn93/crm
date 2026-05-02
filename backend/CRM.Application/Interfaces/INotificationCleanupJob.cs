namespace CRM.Application.Interfaces;

/// <summary>
/// Logic dọn notification cũ + log task notification orphan.
/// </summary>
public interface INotificationCleanupJob
{
    Task RunAsync(CancellationToken ct = default);
}
