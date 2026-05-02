namespace CRM.Application.DTOs.Notification;

public class NotificationFilterDto
{
    public bool UnreadOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
