using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<User>> GetAllWithRolesAsync()
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task AddUserRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
        await _context.UserRoles.AddAsync(userRole);
    }

    public async Task RemoveUserRolesAsync(Guid userId)
    {
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        _context.UserRoles.RemoveRange(userRoles);
    }
}
