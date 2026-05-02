using AutoMapper;
using CRM.Application.Authorization;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Task;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Application.Interfaces;
using TaskStatusEnum = CRM.Core.Enums.TaskStatus;

namespace CRM.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _dispatcher;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper, INotificationDispatcher dispatcher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dispatcher = dispatcher;
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
            filter.CreatedBy,
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
        task.DueDate = AsUtc(task.DueDate);
        task.ReminderDate = AsUtc(task.ReminderDate);

        // If no assigned user specified, assign to creator
        if (!dto.AssignedToUserId.HasValue)
        {
            task.AssignedToUserId = userId;
        }
        else if (dto.AssignedToUserId.Value != userId)
        {
            await EnsureCanAssignAsync(userId, dto.AssignedToUserId.Value);
        }

        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        // Notify assignee nếu khác người tạo
        if (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value != userId)
        {
            var creator = await _unitOfWork.Users.GetByIdAsync(userId);
            await _dispatcher.DispatchAsync(BuildAssignedEvent(task, creator));
        }

        return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Không thể tạo tác vụ.");
    }

    // Postgres timestamptz cần UTC. DateTime nhận từ FE thường có Kind=Unspecified
    // (vd "2026-04-22") — coi như UTC, giữ nguyên ngày user chọn.
    private static DateTime? AsUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    public async Task<TaskDto> UpdateAsync(UpdateTaskDto dto, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.Id);

        if (task == null)
        {
            throw new KeyNotFoundException("Không tìm thấy tác vụ.");
        }

        // Chỉ người tạo (hoặc Admin) được sửa các field; assignee dùng UpdateStatusAsync.
        if (!await IsCreatorOrAdminAsync(task, userId))
        {
            throw new UnauthorizedAccessException("Chỉ người tạo công việc được phép chỉnh sửa.");
        }

        // Validate assignment if assignee changed to someone other than the current user
        if (dto.AssignedToUserId.HasValue
            && dto.AssignedToUserId.Value != userId
            && dto.AssignedToUserId.Value != task.AssignedToUserId)
        {
            await EnsureCanAssignAsync(userId, dto.AssignedToUserId.Value);
        }

        // Snapshot trước khi map để so sánh thay đổi cho notification.
        var oldAssigneeId = task.AssignedToUserId;
        var oldDueDate = task.DueDate;
        var oldStatus = task.Status;

        // If status changed to completed, set CompletedAt
        if (dto.Status == TaskStatusEnum.Completed && task.Status != TaskStatusEnum.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.Status != TaskStatusEnum.Completed)
        {
            task.CompletedAt = null;
        }

        _mapper.Map(dto, task);
        task.DueDate = AsUtc(task.DueDate);
        task.ReminderDate = AsUtc(task.ReminderDate);
        _unitOfWork.Tasks.Update(task);

        // Reset notification log nếu DueDate hoặc Assignee thay đổi (cho phép re-notify với context mới)
        var assigneeChanged = oldAssigneeId != task.AssignedToUserId;
        var dueDateChanged = oldDueDate != task.DueDate;
        if (assigneeChanged || dueDateChanged)
        {
            await _unitOfWork.TaskNotificationLogs.ClearForTaskAsync(task.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        var actor = await _unitOfWork.Users.GetByIdAsync(userId);
        var events = new List<NotificationEvent>();

        // Reassigned: notify new assignee (nếu khác current user) + old assignee
        if (assigneeChanged)
        {
            if (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value != userId)
            {
                events.Add(BuildAssignedEvent(task, actor));
            }
            if (oldAssigneeId.HasValue && oldAssigneeId.Value != userId && oldAssigneeId != task.AssignedToUserId)
            {
                events.Add(BuildReassignedEvent(task, oldAssigneeId.Value, actor));
            }
        }

        // Completed: notify creator (nếu khác current user)
        if (oldStatus != TaskStatusEnum.Completed && task.Status == TaskStatusEnum.Completed
            && task.CreatedByUserId != userId)
        {
            events.Add(BuildCompletedEvent(task, actor));
        }

        if (events.Count > 0)
        {
            await _dispatcher.DispatchManyAsync(events);
        }

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

        // Người tạo, người được giao, và Admin đều được cập nhật trạng thái.
        var isAssignee = task.AssignedToUserId == userId;
        if (!isAssignee && !await IsCreatorOrAdminAsync(task, userId))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật trạng thái công việc này.");
        }

        var oldStatus = task.Status;
        task.Status = dto.Status;

        // If status changed to completed, set CompletedAt
        if (dto.Status == TaskStatusEnum.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync();

        // Notify creator khi assignee complete task
        if (oldStatus != TaskStatusEnum.Completed
            && task.Status == TaskStatusEnum.Completed
            && task.CreatedByUserId != userId)
        {
            var actor = await _unitOfWork.Users.GetByIdAsync(userId);
            await _dispatcher.DispatchAsync(BuildCompletedEvent(task, actor));
        }

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

    public async Task<IEnumerable<AssignableUserDto>> GetAssignableUsersAsync(Guid currentUserId)
    {
        var current = await _unitOfWork.Users.GetByIdWithRolesAsync(currentUserId);
        if (current == null) return Enumerable.Empty<AssignableUserDto>();

        var currentRoles = current.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var assignableRoles = TaskAssignmentRules.GetAssignableTargetRoles(currentRoles);
        if (assignableRoles.Count == 0) return Enumerable.Empty<AssignableUserDto>();

        var allUsers = await _unitOfWork.Users.GetAllWithRolesAsync();
        return allUsers
            .Where(u => u.IsActive && u.Id != currentUserId)
            .Where(u => u.UserRoles.Any(ur => assignableRoles.Contains(ur.Role.Name)))
            .Select(u => new AssignableUserDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToArray()
            })
            .OrderBy(u => u.FullName)
            .ToList();
    }

    private async Task<bool> IsCreatorOrAdminAsync(TaskItem task, Guid userId)
    {
        if (task.CreatedByUserId == userId) return true;
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);
        return user?.UserRoles.Any(ur => ur.Role.Name == RoleNames.Admin) ?? false;
    }

    private async Task EnsureCanAssignAsync(Guid currentUserId, Guid targetUserId)
    {
        var current = await _unitOfWork.Users.GetByIdWithRolesAsync(currentUserId);
        var target = await _unitOfWork.Users.GetByIdWithRolesAsync(targetUserId);

        if (target == null)
            throw new InvalidOperationException("Không tìm thấy người được giao việc.");

        var currentRoles = current?.UserRoles.Select(ur => ur.Role.Name) ?? Enumerable.Empty<string>();
        var targetRoles = target.UserRoles.Select(ur => ur.Role.Name);

        if (!TaskAssignmentRules.CanAssignTo(currentRoles, targetRoles))
            throw new InvalidOperationException("Bạn không có quyền giao việc cho người này.");
    }

    private static NotificationEvent BuildAssignedEvent(TaskItem task, User? actor)
    {
        var dueText = task.DueDate.HasValue
            ? $" (hạn {task.DueDate.Value.ToLocalTime():dd/MM/yyyy HH:mm})"
            : string.Empty;
        var actorName = actor?.FullName ?? "Hệ thống";

        return new NotificationEvent
        {
            Type = NotificationType.TaskAssigned,
            Severity = NotificationSeverity.Info,
            RecipientUserId = task.AssignedToUserId!.Value,
            Title = "Bạn vừa được giao một công việc mới",
            Message = $"{task.Title}{dueText} — giao bởi {actorName}",
            Link = $"/tasks/{task.Id}/edit",
            EntityType = "Task",
            EntityId = task.Id,
            EmailSubject = $"[CRM] Công việc mới: {task.Title}",
            EmailHtmlBody = BuildEmailHtml(
                "Bạn vừa được giao một công việc mới",
                $"<p><strong>{task.Title}</strong></p>"
                + (string.IsNullOrEmpty(task.Description) ? string.Empty : $"<p>{task.Description}</p>")
                + $"<p>Người giao: {actorName}</p>"
                + (task.DueDate.HasValue ? $"<p>Hạn chót: {task.DueDate.Value.ToLocalTime():dd/MM/yyyy HH:mm}</p>" : string.Empty),
                $"/tasks/{task.Id}/edit",
                "Xem công việc")
        };
    }

    private static NotificationEvent BuildReassignedEvent(TaskItem task, Guid oldAssigneeId, User? actor)
    {
        var actorName = actor?.FullName ?? "Hệ thống";
        return new NotificationEvent
        {
            Type = NotificationType.TaskReassigned,
            Severity = NotificationSeverity.Info,
            RecipientUserId = oldAssigneeId,
            Title = "Công việc đã được giao cho người khác",
            Message = $"{task.Title} — chuyển bởi {actorName}",
            Link = $"/tasks/{task.Id}/edit",
            EntityType = "Task",
            EntityId = task.Id
        };
    }

    private static NotificationEvent BuildCompletedEvent(TaskItem task, User? actor)
    {
        var actorName = actor?.FullName ?? "Người được giao";
        return new NotificationEvent
        {
            Type = NotificationType.TaskCompleted,
            Severity = NotificationSeverity.Success,
            RecipientUserId = task.CreatedByUserId,
            Title = "Công việc đã được hoàn thành",
            Message = $"{task.Title} — hoàn thành bởi {actorName}",
            Link = $"/tasks/{task.Id}/edit",
            EntityType = "Task",
            EntityId = task.Id
        };
    }

    private static string BuildEmailHtml(string heading, string bodyHtml, string relativeLink, string ctaText)
    {
        // Body link sẽ được resolve ở email layer khi cần FrontendBaseUrl. Hiện tại chỉ là placeholder.
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;"">
  <h2 style=""color: #2563eb;"">{heading}</h2>
  {bodyHtml}
  <p style=""margin-top: 24px;"">
    <a href=""{relativeLink}"" style=""display:inline-block;padding:10px 20px;background:#2563eb;color:#fff;text-decoration:none;border-radius:6px;"">{ctaText}</a>
  </p>
  <hr style=""margin-top:32px;border:none;border-top:1px solid #e5e7eb;"">
  <p style=""color: #6b7280; font-size: 12px;"">CRM Đồng Phục Bốn Mùa — vui lòng không trả lời email này.</p>
</body>
</html>";
    }
}
