using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Verdiq.API.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private static readonly ConcurrentDictionary<string, UserPresence> ConnectedUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";
            var role = Context.User?.FindFirst("role")?.Value ?? "User";

            ConnectedUsers[userId] = new UserPresence
            {
                UserId = userId,
                UserName = userName,
                Role = role,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow
            };

            await Groups.AddToGroupAsync(Context.ConnectionId, "presence_tracker");
            await Clients.Others.SendAsync("UserPresenceChanged", new
            {
                userId,
                userName,
                role,
                status = "online",
                connectedAt = DateTime.UtcNow
            });
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            ConnectedUsers.TryRemove(userId, out _);
            await Clients.Others.SendAsync("UserPresenceChanged", new
            {
                userId,
                status = "offline"
            });
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<List<UserPresence>> GetOnlineUsers()
    {
        return ConnectedUsers.Values.ToList();
    }

    public async Task SetActivity(string activity)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId) && ConnectedUsers.TryGetValue(userId, out var presence))
        {
            presence.CurrentActivity = activity;
            presence.LastActiveAt = DateTime.UtcNow;
        }
    }
}

public class UserPresence
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string? CurrentActivity { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
}
