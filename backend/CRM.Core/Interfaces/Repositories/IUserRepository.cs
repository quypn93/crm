using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(Guid id);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllWithRolesAsync();
    Task AddUserRoleAsync(Guid userId, Guid roleId);
    Task RemoveUserRolesAsync(Guid userId);
}
