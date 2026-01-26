using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CRM.Application.DTOs.Auth;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email already exists
        if (await _unitOfWork.Users.ExistsAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            throw new InvalidOperationException("Email đã được sử dụng.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        await _unitOfWork.Users.AddAsync(user);

        // Assign default role (SalesRep)
        var salesRepRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        await _unitOfWork.Users.AddUserRoleAsync(user.Id, salesRepRoleId);

        await _unitOfWork.SaveChangesAsync();

        // Reload user with roles
        user = await _unitOfWork.Users.GetByIdWithRolesAsync(user.Id);

        return CreateAuthResponse(user!);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");
        }

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var expiresIn = int.Parse(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken!,
            ExpiresIn = expiresIn * 60, // Convert to seconds
            User = _mapper.Map<UserDto>(user)
        };
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        // Add role claims
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var expiresInMinutes = int.Parse(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
