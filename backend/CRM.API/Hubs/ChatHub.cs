using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // userId -> số connection đang mở. Dùng để biết user còn kết nối nào không khi 1 tab disconnect.
    // ConcurrentDictionary an toàn cho multi-thread; key là Guid để tránh string-allocation.
    private static readonly ConcurrentDictionary<Guid, int> _connectionsByUser = new();

    /// <summary>Group chung tất cả user đang online — để broadcast presence change.</summary>
    public const string PresenceGroup = "chat-presence";

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(userId.Value));
            await Groups.AddToGroupAsync(Context.ConnectionId, PresenceGroup);

            var newCount = _connectionsByUser.AddOrUpdate(userId.Value, 1, (_, v) => v + 1);
            if (newCount == 1)
            {
                // Lần đầu user này có connection → báo cho mọi người là user online.
                await Clients.Group(PresenceGroup).SendAsync("chatPresence", new
                {
                    userId = userId.Value,
                    isOnline = true
                });
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(userId.Value));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PresenceGroup);

            var remaining = _connectionsByUser.AddOrUpdate(userId.Value, 0, (_, v) => Math.Max(0, v - 1));
            if (remaining == 0)
            {
                _connectionsByUser.TryRemove(userId.Value, out _);
                await Clients.Group(PresenceGroup).SendAsync("chatPresence", new
                {
                    userId = userId.Value,
                    isOnline = false
                });
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    private Guid? GetUserId()
    {
        var raw = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static string GroupName(string userId) => $"chat-user-{userId}";
    public static string GroupName(Guid userId) => $"chat-user-{userId}";

    /// <summary>Trả về snapshot user-ids đang online — cho REST endpoint dùng khi user load trang.</summary>
    public static IReadOnlyCollection<Guid> GetOnlineUserIds()
    {
        return _connectionsByUser.Keys.ToArray();
    }

    public static bool IsOnline(Guid userId) => _connectionsByUser.ContainsKey(userId);
}
