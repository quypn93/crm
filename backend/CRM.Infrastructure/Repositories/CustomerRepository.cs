using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.CreatedByUser)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Deals)
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? assignedTo,
        bool? isActive,
        string? industry,
        string? city,
        DateTime? createdFrom,
        DateTime? createdTo,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet
            .Include(c => c.CreatedByUser)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Deals)
            .Include(c => c.Tasks)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)));
        }

        if (assignedTo.HasValue)
            query = query.Where(c => c.AssignedToUserId == assignedTo.Value);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(industry))
            query = query.Where(c => c.Industry == industry);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(c => c.City != null && c.City.ToLower().Contains(city.ToLower()));

        if (createdFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= createdFrom.Value);

        if (createdTo.HasValue)
            query = query.Where(c => c.CreatedAt <= createdTo.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name),
            "email" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(c => c.Email)
                : query.OrderByDescending(c => c.Email),
            "companyname" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(c => c.CompanyName)
                : query.OrderByDescending(c => c.CompanyName),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Customer>> GetByAssignedUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.Deals)
            .Include(c => c.Tasks)
            .Where(c => c.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<int> GetCustomerCountByIndustryAsync(string industry)
    {
        return await _dbSet.CountAsync(c => c.Industry == industry);
    }
}
