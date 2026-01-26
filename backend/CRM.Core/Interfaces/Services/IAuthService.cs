using CRM.Application.DTOs.Auth;

namespace CRM.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(Guid userId);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}
