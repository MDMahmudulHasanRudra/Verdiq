using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Verdiq.Application.DTOs.Notification;

namespace Verdiq.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, "all_users");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinCaseGroup(string caseId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"case_{caseId}");
    }

    public async Task LeaveCaseGroup(string caseId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"case_{caseId}");
    }
}

public static class NotificationHubMethods
{
    public const string NotificationReceived = "NotificationReceived";
    public const string UnreadCountChanged = "UnreadCountChanged";
    public const string ActivityCreated = "ActivityCreated";
    public const string CaseUpdated = "CaseUpdated";
    public const string DocumentUpdated = "DocumentUpdated";
    public const string HearingUpdated = "HearingUpdated";
    public const string UserPresenceChanged = "UserPresenceChanged";
    public const string ForceLogout = "ForceLogout";
}
