using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.User;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>Lấy danh sách user (có phân trang, tìm kiếm, lọc)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedUsersResult>>> GetUsers([FromQuery] UserSearchParams searchParams)
    {
        var result = await _userManagementService.GetUsersAsync(searchParams);
        return Ok(ApiResponse<PagedUsersResult>.Ok(result));
    }

    /// <summary>Lấy chi tiết một user</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> GetUser(Guid id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        if (user == null) return NotFound(ApiResponse<UserListItemDto>.Fail("Không tìm thấy user."));
        return Ok(ApiResponse<UserListItemDto>.Ok(user));
    }

    /// <summary>Tạo user mới</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userManagementService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ApiResponse<UserListItemDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserListItemDto>.Fail(ex.Message));
        }
    }

    /// <summary>Cập nhật thông tin user</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userManagementService.UpdateUserAsync(id, dto);
            return Ok(ApiResponse<UserListItemDto>.Ok(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail(ex.Message));
        }
    }

    /// <summary>Xóa user</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
            return BadRequest(ApiResponse.Fail("Không thể xóa tài khoản của chính mình."));

        try
        {
            await _userManagementService.DeleteUserAsync(id);
            return Ok(ApiResponse.Ok("Đã xóa người dùng."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>Bật/tắt trạng thái hoạt động của user</summary>
    [HttpPut("{id:guid}/toggle-active")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> ToggleActive(Guid id)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
            return BadRequest(ApiResponse<UserListItemDto>.Fail("Không thể vô hiệu hóa tài khoản của chính mình."));

        try
        {
            var user = await _userManagementService.ToggleActiveAsync(id);
            return Ok(ApiResponse<UserListItemDto>.Ok(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail(ex.Message));
        }
    }

    /// <summary>Gán roles cho user</summary>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<UserListItemDto>>> AssignRoles(Guid id, [FromBody] AssignRolesDto dto)
    {
        try
        {
            var user = await _userManagementService.AssignRolesAsync(id, dto);
            return Ok(ApiResponse<UserListItemDto>.Ok(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserListItemDto>.Fail(ex.Message));
        }
    }

    /// <summary>Lấy danh sách tất cả roles</summary>
    [HttpGet("roles")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles()
    {
        var roles = await _userManagementService.GetRolesAsync();
        return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
    }

    /// <summary>Tạo role mới</summary>
    [HttpPost("roles")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleDto dto)
    {
        try
        {
            var role = await _userManagementService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, ApiResponse<RoleDto>.Ok(role));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RoleDto>.Fail(ex.Message));
        }
    }

    /// <summary>Lấy chi tiết một role</summary>
    [HttpGet("roles/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRole(Guid id)
    {
        var role = await _userManagementService.GetRoleByIdAsync(id);
        if (role == null) return NotFound(ApiResponse<RoleDto>.Fail("Không tìm thấy vai trò."));
        return Ok(ApiResponse<RoleDto>.Ok(role));
    }

    /// <summary>Cập nhật mô tả vai trò</summary>
    [HttpPut("roles/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _userManagementService.UpdateRoleAsync(id, dto);
            return Ok(ApiResponse<RoleDto>.Ok(role));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.Fail(ex.Message));
        }
    }

    /// <summary>Xóa role</summary>
    [HttpDelete("roles/{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse>> DeleteRole(Guid id)
    {
        try
        {
            await _userManagementService.DeleteRoleAsync(id);
            return Ok(ApiResponse.Ok("Đã xóa vai trò."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
