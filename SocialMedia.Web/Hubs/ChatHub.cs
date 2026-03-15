using Microsoft.AspNetCore.SignalR;

namespace SocialMedia.Web.Hubs;

/// <summary>
/// SignalR hub for real-time chat messaging.
/// </summary>
public class ChatHub : Hub
{
    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    /// <summary>
    /// Sends a message to a specific group (chat room).
    /// </summary>
    public async Task SendMessageToGroup(string groupName, string user, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
    }

    /// <summary>
    /// Adds the current connection to a chat group.
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes the current connection from a chat group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", Context.ConnectionId, groupName);
    }
}
