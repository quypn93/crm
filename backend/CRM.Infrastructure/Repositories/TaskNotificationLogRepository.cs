using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class TaskNotificationLogRepository : Repository<TaskNotificationLog>, ITaskNotificationLogRepository
{
    public TaskNotificationLogRepository(CrmDbContext context) : base(context)
    {
    }

    public Task<bool> HasLogAsync(Guid taskId, NotificationType type)
    {
        return _dbSet.AnyAsync(l => l.TaskId == taskId && l.Type == type);
    }

    public async Task ClearForTaskAsync(Guid taskId)
    {
        await _dbSet
            .Where(l => l.TaskId == taskId)
            .ExecuteDeleteAsync();
    }
}
