using CRM.Core.Enums;
using TaskStatusEnum = CRM.Core.Enums.TaskStatus;

namespace CRM.Application.DTOs.Task;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public TaskPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public TaskStatusEnum Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DealId { get; set; }
    public string? DealTitle { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskStatusEnum.Completed;
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? CustomerId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class UpdateTaskDto : CreateTaskDto
{
    public Guid Id { get; set; }
    public TaskStatusEnum Status { get; set; }
}

public class UpdateTaskStatusDto
{
    public Guid TaskId { get; set; }
    public TaskStatusEnum Status { get; set; }
}

public class TaskFilterDto
{
    public string? Search { get; set; }
    public TaskStatusEnum? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? IsOverdue { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}

public static class TaskPriorityNames
{
    public static string GetName(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "Thap",
        TaskPriority.Medium => "Trung binh",
        TaskPriority.High => "Cao",
        TaskPriority.Urgent => "Khan cap",
        _ => "Khong xac dinh"
    };
}

public static class TaskStatusNames
{
    public static string GetName(TaskStatusEnum status) => status switch
    {
        TaskStatusEnum.Pending => "Cho xu ly",
        TaskStatusEnum.InProgress => "Dang thuc hien",
        TaskStatusEnum.Completed => "Hoan thanh",
        TaskStatusEnum.Cancelled => "Da huy",
        _ => "Khong xac dinh"
    };
}
