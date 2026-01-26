using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Task;

namespace CRM.Core.Interfaces.Services;

public interface ITaskService
{
    Task<TaskDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<TaskDto>> GetPagedAsync(TaskFilterDto filter);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, Guid userId);
    Task<TaskDto> UpdateAsync(UpdateTaskDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<TaskDto> UpdateStatusAsync(UpdateTaskStatusDto dto, Guid userId);
    Task<IEnumerable<TaskDto>> GetMyTasksAsync(Guid userId);
    Task<IEnumerable<TaskDto>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskDto>> GetTasksDueTodayAsync();
    Task<IEnumerable<TaskDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<TaskDto>> GetByDealAsync(Guid dealId);
}
