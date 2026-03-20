using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialMedia.Web.Hubs;

/// <summary>
/// Real-time notification hub.
/// Uses ASP.NET Identity's UserIdentifier so Clients.User(userId) works out of the box.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Push a notification to a specific user from a server-side service (via IHubContext).
    /// Not typically called directly by clients.
    /// </summary>
    public async Task NotifyUser(string targetUserId, string type, string message, string? actionUrl = null)
    {
        await Clients.Group(targetUserId).SendAsync("ReceiveNotification", new
        {
            type,
            message,
            actionUrl,
            createdAt = DateTime.UtcNow
        });
    }
}
