using CRM.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CRM.Infrastructure.Services.Email;

/// <summary>
/// SMTP email sender qua MailKit. Áp dụng cho Google Workspace (smtp.gmail.com:587 + App Password)
/// hoặc bất kỳ SMTP server nào hỗ trợ STARTTLS / SSL.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("SmtpEmailSender invoked nhưng config không đầy đủ — skip.");
            return;
        }

        if (string.IsNullOrWhiteSpace(toAddress))
        {
            _logger.LogWarning("SmtpEmailSender: địa chỉ nhận rỗng — skip.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName ?? string.Empty, _options.FromAddress));
        message.To.Add(new MailboxAddress(toName ?? string.Empty, toAddress));
        if (!string.IsNullOrWhiteSpace(_options.ReplyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(_options.ReplyTo));
        }
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        // SecureSocketOptions:
        // - 587 + UseStartTls = true → StartTls
        // - 465 → SslOnConnect
        // - others → Auto
        var secureOption = _options.UseStartTls && _options.Port == 587
            ? SecureSocketOptions.StartTls
            : _options.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.Auto;

        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, secureOption, ct);
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent: To={To} Subject={Subject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP send thất bại: To={To} Subject={Subject}", toAddress, subject);
            throw;
        }
    }
}
