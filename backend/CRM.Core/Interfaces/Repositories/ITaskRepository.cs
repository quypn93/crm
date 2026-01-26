using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetByIdWithDetailsAsync(Guid id);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
        string? search,
        TaskStatus? status,
        TaskPriority? priority,
        Guid? customerId,
        Guid? dealId,
        Guid? assignedTo,
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
}
