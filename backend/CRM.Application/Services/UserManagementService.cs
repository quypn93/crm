using AutoMapper;
using CRM.Application.DTOs.User;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserManagementService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedUsersResult> GetUsersAsync(UserSearchParams searchParams)
    {
        var users = (await _unitOfWork.Users.GetAllWithRolesAsync()).AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            var term = searchParams.SearchTerm.ToLower();
            users = users.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(searchParams.Role))
        {
            users = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == searchParams.Role));
        }

        // Filter by active status
        if (searchParams.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == searchParams.IsActive.Value);
        }

        var totalCount = users.Count();
        var items = users
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((searchParams.Page - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToList();

        return new PagedUsersResult
        {
            Items = _mapper.Map<List<UserListItemDto>>(items),
            TotalCount = totalCount,
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)searchParams.PageSize)
        };
    }

    public async Task<UserListItemDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id);
        return user == null ? null : _mapper.Map<UserListItemDto>(user);
    }

    public async Task<UserListItemDto> CreateUserAsync(CreateUserDto dto)
    {
        // Check email uniqueness
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng.");

        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign roles
        if (dto.Roles.Any())
        {
            await AssignRolesToUserAsync(user.Id, dto.Roles);
            await _unitOfWork.SaveChangesAsync();
        }

        var created = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id);
        return _mapper.Map<UserListItemDto>(created!);
    }

    public async Task<UserListItemDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy user với id '{id}'.");

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.IsActive = dto.IsActive;

        _unitOfWork.Users.Update(user);

        // Re-assign roles
        await _unitOfWork.Users.RemoveUserRolesAsync(id);
        await _unitOfWork.SaveChangesAsync();

        if (dto.Roles.Any())
        {
            await AssignRolesToUserAsync(id, dto.Roles);
            await _unitOfWork.SaveChangesAsync();
        }

        var updated = await _unitOfWork.Users.GetByIdWithRolesAsync(id);
        return _mapper.Map<UserListItemDto>(updated!);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy user với id '{id}'.");

        await _unitOfWork.Users.RemoveUserRolesAsync(id);
        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserListItemDto> ToggleActiveAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy user với id '{id}'.");

        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserListItemDto>(user);
    }

    public async Task<UserListItemDto> AssignRolesAsync(Guid id, AssignRolesDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy user với id '{id}'.");

        await _unitOfWork.Users.RemoveUserRolesAsync(id);
        await _unitOfWork.SaveChangesAsync();

        if (dto.Roles.Any())
        {
            await AssignRolesToUserAsync(id, dto.Roles);
            await _unitOfWork.SaveChangesAsync();
        }

        var updated = await _unitOfWork.Users.GetByIdWithRolesAsync(id);
        return _mapper.Map<UserListItemDto>(updated!);
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return _mapper.Map<List<RoleDto>>(roles);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        return role == null ? null : _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var name = dto.Name.Trim();

        // Check name uniqueness
        var existing = await _unitOfWork.Roles.GetByNameAsync(name);
        if (existing != null)
            throw new InvalidOperationException($"Vai trò '{name}' đã tồn tại.");

        var role = new Role
        {
            Name = name,
            Description = dto.Description?.Trim()
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy vai trò với id '{id}'.");

        role.Description = dto.Description?.Trim();
        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteRoleAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy vai trò với id '{id}'.");

        // Prevent deleting built-in roles
        var builtIn = RoleNames.AllRoles;
        if (builtIn.Contains(role.Name))
            throw new InvalidOperationException($"Không thể xóa vai trò mặc định '{role.Name}'.");

        _unitOfWork.Roles.Remove(role);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task AssignRolesToUserAsync(Guid userId, List<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
            if (role != null)
            {
                await _unitOfWork.Users.AddUserRoleAsync(userId, role.Id);
            }
        }
    }
}
