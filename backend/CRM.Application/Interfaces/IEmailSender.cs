namespace CRM.Application.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken ct = default);
}
