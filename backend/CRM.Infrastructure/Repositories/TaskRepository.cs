using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskStatusEnum = CRM.Core.Enums.TaskStatus;

namespace CRM.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Deal)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
        string? search,
        TaskStatusEnum? status,
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
        string sortOrder)
    {
        var query = _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Deal)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (customerId.HasValue)
            query = query.Where(t => t.CustomerId == customerId.Value);

        if (dealId.HasValue)
            query = query.Where(t => t.DealId == dealId.Value);

        if (assignedTo.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedTo.Value);

        if (createdBy.HasValue)
            query = query.Where(t => t.CreatedByUserId == createdBy.Value);

        if (dueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= dueDateTo.Value);

        if (isOverdue.HasValue && isOverdue.Value)
            query = query.Where(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < DateTime.UtcNow &&
                t.Status != TaskStatusEnum.Completed);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
            "duedate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
            "priority" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
            "status" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.Status)
                : query.OrderByDescending(t => t.Status),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<TaskItem>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet
            .Include(t => t.Deal)
            .Include(t => t.AssignedToUser)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByDealAsync(Guid dealId)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Where(t => t.DealId == dealId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Deal)
            .Where(t => t.AssignedToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Deal)
            .Include(t => t.AssignedToUser)
            .Where(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < DateTime.UtcNow &&
                t.Status != TaskStatusEnum.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetTasksDueTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Deal)
            .Include(t => t.AssignedToUser)
            .Where(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value >= today &&
                t.DueDate.Value < tomorrow &&
                t.Status != TaskStatusEnum.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<int> GetPendingTasksCountAsync(Guid? userId = null)
    {
        var query = _dbSet.Where(t => t.Status == TaskStatusEnum.Pending || t.Status == TaskStatusEnum.InProgress);

        if (userId.HasValue)
            query = query.Where(t => t.AssignedToUserId == userId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetOverdueTasksCountAsync(Guid? userId = null)
    {
        var query = _dbSet.Where(t =>
            t.DueDate.HasValue &&
            t.DueDate.Value < DateTime.UtcNow &&
            t.Status != TaskStatusEnum.Completed);

        if (userId.HasValue)
            query = query.Where(t => t.AssignedToUserId == userId.Value);

        return await query.CountAsync();
    }
}
