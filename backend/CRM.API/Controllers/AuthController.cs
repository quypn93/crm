using System.Security.Claims;
using CRM.Application.DTOs.Auth;
using CRM.Application.DTOs.Common;
using CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng ký thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userId = GetCurrentUserId();
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse.Ok("Đăng xuất thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetCurrentUserAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy người dùng."));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponse.Ok("Đổi mật khẩu thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
