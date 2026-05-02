using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface ITaskNotificationLogRepository : IRepository<TaskNotificationLog>
{
    Task<bool> HasLogAsync(Guid taskId, NotificationType type);

    Task ClearForTaskAsync(Guid taskId);
}
