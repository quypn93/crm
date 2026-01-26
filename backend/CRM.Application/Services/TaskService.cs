using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Task;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Services;

namespace CRM.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(id);
        return task != null ? _mapper.Map<TaskDto>(task) : null;
    }

    public async Task<PaginatedResult<TaskDto>> GetPagedAsync(TaskFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Tasks.GetPagedAsync(
            filter.Search,
            filter.Status,
            filter.Priority,
            filter.CustomerId,
            filter.DealId,
            filter.AssignedTo,
            filter.DueDateFrom,
            filter.DueDateTo,
            filter.IsOverdue,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<TaskDto>>(items);
        return PaginatedResult<TaskDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, Guid userId)
    {
        var task = _mapper.Map<TaskItem>(dto);
        task.CreatedByUserId = userId;

        // If no assigned user specified, assign to creator
        if (!dto.AssignedToUserId.HasValue)
        {
            task.AssignedToUserId = userId;
        }

        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Không thể tạo tác vụ.");
    }

    public async Task<TaskDto> UpdateAsync(UpdateTaskDto dto, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.Id);

        if (task == null)
        {
            throw new KeyNotFoundException("Không tìm thấy tác vụ.");
        }

        // If status changed to completed, set CompletedAt
        if (dto.Status == TaskStatus.Completed && task.Status != TaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.Status != TaskStatus.Completed)
        {
            task.CompletedAt = null;
        }

        _mapper.Map(dto, task);
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Không thể cập nhật tác vụ.");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);

        if (task == null)
        {
            throw new KeyNotFoundException("Không tìm thấy tác vụ.");
        }

        _unitOfWork.Tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TaskDto> UpdateStatusAsync(UpdateTaskStatusDto dto, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);

        if (task == null)
        {
            throw new KeyNotFoundException("Không tìm thấy tác vụ.");
        }

        task.Status = dto.Status;

        // If status changed to completed, set CompletedAt
        if (dto.Status == TaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Không thể cập nhật tác vụ.");
    }

    public async Task<IEnumerable<TaskDto>> GetMyTasksAsync(Guid userId)
    {
        var tasks = await _unitOfWork.Tasks.GetByAssignedUserAsync(userId);
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync()
    {
        var tasks = await _unitOfWork.Tasks.GetOverdueTasksAsync();
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<IEnumerable<TaskDto>> GetTasksDueTodayAsync()
    {
        var tasks = await _unitOfWork.Tasks.GetTasksDueTodayAsync();
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<IEnumerable<TaskDto>> GetByCustomerAsync(Guid customerId)
    {
        var tasks = await _unitOfWork.Tasks.GetByCustomerAsync(customerId);
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<IEnumerable<TaskDto>> GetByDealAsync(Guid dealId)
    {
        var tasks = await _unitOfWork.Tasks.GetByDealAsync(dealId);
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }
}
