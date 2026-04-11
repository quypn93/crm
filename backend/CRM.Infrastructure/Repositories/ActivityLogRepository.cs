using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityLog>> GetRecentActivitiesAsync(int count)
    {
        return await _dbSet
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
