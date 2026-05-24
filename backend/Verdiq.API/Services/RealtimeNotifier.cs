using Microsoft.AspNetCore.SignalR;
using Verdiq.API.Hubs;

namespace Verdiq.API.Services;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(string userId, string method, object? payload = null);
    Task NotifyCaseGroupAsync(string caseId, string method, object? payload = null);
    Task NotifyAllAsync(string method, object? payload = null);
    Task BroadcastActivityAsync(string title, string description, string type, string? referenceId = null);
}

public class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealtimeNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(string userId, string method, object? payload = null)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync(method, payload);
    }

    public async Task NotifyCaseGroupAsync(string caseId, string method, object? payload = null)
    {
        await _hubContext.Clients.Group($"case_{caseId}").SendAsync(method, payload);
    }

    public async Task NotifyAllAsync(string method, object? payload = null)
    {
        await _hubContext.Clients.All.SendAsync(method, payload);
    }

    public async Task BroadcastActivityAsync(string title, string description, string type, string? referenceId = null)
    {
        var payload = new
        {
            id = Guid.NewGuid().ToString(),
            title,
            description,
            type,
            referenceId,
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group("all_users").SendAsync(NotificationHubMethods.ActivityCreated, payload);
    }
}
