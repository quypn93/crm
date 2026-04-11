using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<IEnumerable<ActivityLog>> GetRecentActivitiesAsync(int count);
}
