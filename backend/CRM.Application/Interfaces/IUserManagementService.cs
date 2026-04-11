using CRM.Application.DTOs.User;

namespace CRM.Application.Interfaces;

public interface IUserManagementService
{
    Task<PagedUsersResult> GetUsersAsync(UserSearchParams searchParams);
    Task<UserListItemDto?> GetUserByIdAsync(Guid id);
    Task<UserListItemDto> CreateUserAsync(CreateUserDto dto);
    Task<UserListItemDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task DeleteUserAsync(Guid id);
    Task<UserListItemDto> ToggleActiveAsync(Guid id);
    Task<UserListItemDto> AssignRolesAsync(Guid id, AssignRolesDto dto);
    Task<List<RoleDto>> GetRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
    Task DeleteRoleAsync(Guid id);
}
