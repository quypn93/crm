using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Email;

/// <summary>
/// Fallback IEmailSender khi cấu hình SMTP rỗng — log warning và bỏ qua thay vì throw.
/// Cho phép dev chạy hệ thống mà không cần SMTP credentials. Phase 1C sẽ swap sang SmtpEmailSender qua DI.
/// </summary>
public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;
    private bool _warned;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_warned)
        {
            _logger.LogWarning("Email chưa được cấu hình — notification email sẽ bị bỏ qua. " +
                "Điền Email:Username/Password trong appsettings để bật SMTP.");
            _warned = true;
        }

        _logger.LogInformation("Email skipped (NoOp): To={To} Subject={Subject}", toAddress, subject);
        return Task.CompletedTask;
    }
}
