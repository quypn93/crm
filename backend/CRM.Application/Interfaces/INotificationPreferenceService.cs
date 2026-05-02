using CRM.Application.DTOs.Notification;
using CRM.Core.Enums;

namespace CRM.Application.Interfaces;

public class ResolvedPreference
{
    public bool InApp { get; set; }
    public bool Email { get; set; }
}

public interface INotificationPreferenceService
{
    /// <summary>
    /// Resolve preference cho user dựa trên tất cả role họ thuộc.
    /// OR logic: chỉ cần 1 role bật InApp/Email là user nhận.
    /// </summary>
    Task<ResolvedPreference> ResolveForUserAsync(Guid userId, NotificationType type);

    /// <summary>
    /// Lấy toàn bộ preferences cho UI admin (matrix Role × Type).
    /// Bao gồm cả row default từ config (đánh dấu IsDefault = true).
    /// </summary>
    Task<IEnumerable<NotificationRolePreferenceDto>> GetAllAsync();

    Task UpdateAsync(UpdateRolePreferencesRequest request);

    Task ResetToDefaultsAsync();
}
