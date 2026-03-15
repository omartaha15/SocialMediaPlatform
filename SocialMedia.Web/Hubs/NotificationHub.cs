using Microsoft.AspNetCore.SignalR;

namespace SocialMedia.Web.Hubs;

/// <summary>
/// SignalR hub for real-time notifications.
/// </summary>
public class NotificationHub : Hub
{
    /// <summary>
    /// Sends a notification to a specific user by their connection ID.
    /// </summary>
    public async Task SendNotification(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    /// <summary>
    /// Broadcasts a notification to all connected clients.
    /// </summary>
    public async Task BroadcastNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
