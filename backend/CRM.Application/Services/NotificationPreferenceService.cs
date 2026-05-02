using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace CRM.Application.Services;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private const string FallbackAllKey = "_FALLBACK_ALL";

    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationOptions _options;

    public NotificationPreferenceService(IUnitOfWork unitOfWork, IOptions<NotificationOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<ResolvedPreference> ResolveForUserAsync(Guid userId, NotificationType type)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);
        if (user == null) return new ResolvedPreference { InApp = false, Email = false };

        var resolved = new ResolvedPreference();

        // Lấy DB overrides cho mọi role của user
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var dbPrefs = (await _unitOfWork.NotificationRolePreferences.GetByRoleIdsAsync(roleIds))
            .Where(p => p.Type == type)
            .ToDictionary(p => p.RoleId);

        foreach (var ur in user.UserRoles)
        {
            var roleName = ur.Role?.Name ?? string.Empty;

            ChannelConfig config;
            if (dbPrefs.TryGetValue(ur.RoleId, out var dbPref))
            {
                config = new ChannelConfig { InApp = dbPref.InApp, Email = dbPref.Email };
            }
            else
            {
                config = ResolveDefault(roleName, type);
            }

            // OR logic: bất kỳ role nào bật là user nhận
            if (config.InApp) resolved.InApp = true;
            if (config.Email) resolved.Email = true;

            if (resolved.InApp && resolved.Email) break; // short-circuit
        }

        return resolved;
    }

    public async Task<IEnumerable<NotificationRolePreferenceDto>> GetAllAsync()
    {
        var allRoles = (await _unitOfWork.Roles.GetAllAsync()).ToList();
        var dbPrefs = (await _unitOfWork.NotificationRolePreferences.GetAllAsync())
            .ToDictionary(p => (p.RoleId, p.Type));

        var allTypes = Enum.GetValues<NotificationType>();
        var result = new List<NotificationRolePreferenceDto>();

        foreach (var role in allRoles)
        {
            foreach (var type in allTypes)
            {
                if (dbPrefs.TryGetValue((role.Id, type), out var dbPref))
                {
                    result.Add(new NotificationRolePreferenceDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        Type = type,
                        InApp = dbPref.InApp,
                        Email = dbPref.Email,
                        IsDefault = false
                    });
                }
                else
                {
                    var def = ResolveDefault(role.Name, type);
                    result.Add(new NotificationRolePreferenceDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        Type = type,
                        InApp = def.InApp,
                        Email = def.Email,
                        IsDefault = true
                    });
                }
            }
        }

        return result;
    }

    public async Task UpdateAsync(UpdateRolePreferencesRequest request)
    {
        if (request.Items.Count == 0) return;

        var roleIds = request.Items.Select(i => i.RoleId).Distinct().ToList();
        var existing = (await _unitOfWork.NotificationRolePreferences.GetByRoleIdsAsync(roleIds))
            .ToDictionary(p => (p.RoleId, p.Type));

        foreach (var item in request.Items)
        {
            if (existing.TryGetValue((item.RoleId, item.Type), out var pref))
            {
                pref.InApp = item.InApp;
                pref.Email = item.Email;
                _unitOfWork.NotificationRolePreferences.Update(pref);
            }
            else
            {
                await _unitOfWork.NotificationRolePreferences.AddAsync(new NotificationRolePreference
                {
                    RoleId = item.RoleId,
                    Type = item.Type,
                    InApp = item.InApp,
                    Email = item.Email
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ResetToDefaultsAsync()
    {
        var all = await _unitOfWork.NotificationRolePreferences.GetAllAsync();
        foreach (var pref in all)
        {
            _unitOfWork.NotificationRolePreferences.Remove(pref);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private ChannelConfig ResolveDefault(string roleName, NotificationType type)
    {
        if (!_options.RoleDefaults.TryGetValue(roleName, out var roleConfig))
        {
            return new ChannelConfig { InApp = true, Email = false };
        }

        if (roleConfig.TryGetValue(type.ToString(), out var typeConfig))
        {
            return typeConfig;
        }

        if (roleConfig.TryGetValue(FallbackAllKey, out var fallback))
        {
            return fallback;
        }

        return new ChannelConfig { InApp = true, Email = false };
    }
}
