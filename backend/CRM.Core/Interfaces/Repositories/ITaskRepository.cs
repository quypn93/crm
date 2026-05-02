using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetByIdWithDetailsAsync(Guid id);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Enums.TaskStatus? status,
        TaskPriority? priority,
        Guid? customerId,
        Guid? dealId,
        Guid? assignedTo,
        Guid? createdBy,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        bool? isOverdue,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
    Task<IEnumerable<TaskItem>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<TaskItem>> GetByDealAsync(Guid dealId);
    Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskItem>> GetTasksDueTodayAsync();
    Task<int> GetPendingTasksCountAsync(Guid? userId = null);
    Task<int> GetOverdueTasksCountAsync(Guid? userId = null);

    /// <summary>
    /// Tasks có DueDate trong [now, horizon], chưa Completed, có Assignee, và CHƯA có log notification loại đã cho.
    /// Dùng cho background reminder để tránh gửi trùng.
    /// </summary>
    Task<IEnumerable<TaskItem>> GetDueSoonNotNotifiedAsync(DateTime now, DateTime horizon, NotificationType logType);

    /// <summary>
    /// Tasks có DueDate &lt; now, chưa Completed, có Assignee, và CHƯA có log notification loại đã cho.
    /// </summary>
    Task<IEnumerable<TaskItem>> GetOverdueNotNotifiedAsync(DateTime now, NotificationType logType);
}
