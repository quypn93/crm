using CRM.Core.Enums;

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
    public TaskStatus Status { get; set; }
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
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskStatus.Completed;
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
    public TaskStatus Status { get; set; }
}

public class UpdateTaskStatusDto
{
    public Guid TaskId { get; set; }
    public TaskStatus Status { get; set; }
}

public class TaskFilterDto
{
    public string? Search { get; set; }
    public TaskStatus? Status { get; set; }
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
        TaskPriority.Low => "Thấp",
        TaskPriority.Medium => "Trung bình",
        TaskPriority.High => "Cao",
        TaskPriority.Urgent => "Khẩn cấp",
        _ => "Không xác định"
    };
}

public static class TaskStatusNames
{
    public static string GetName(TaskStatus status) => status switch
    {
        TaskStatus.Pending => "Chờ xử lý",
        TaskStatus.InProgress => "Đang thực hiện",
        TaskStatus.Completed => "Hoàn thành",
        TaskStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
}
