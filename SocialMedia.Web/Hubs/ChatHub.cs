using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SocialMedia.Web.Hubs;

/// <summary>
/// Real-time chat hub.
/// Tracks UserId <-> ConnectionId so we can send to specific users.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    // Thread-safe map: UserId -> list of ConnectionIds (user may have multiple tabs)
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    // ── Lifecycle ───────────────────────────────────────────────────────────
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            _userConnections.AddOrUpdate(
                userId,
                _ => [Context.ConnectionId],
                (_, set) => { lock (set) { set.Add(Context.ConnectionId); } return set; });

            // Join a personal group named after the userId for easy targeting
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null && _userConnections.TryGetValue(userId, out var set))
        {
            lock (set) { set.Remove(Context.ConnectionId); }
            if (set.Count == 0)
                _userConnections.TryRemove(userId, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── Direct Message ──────────────────────────────────────────────────────
    /// <summary>
    /// Sends a real-time message to a specific user by their userId.
    /// The caller should also persist the message via the MessagingController.
    /// </summary>
    public async Task SendDirectMessage(string receiverId, string senderName, string content)
    {
        var senderId = Context.UserIdentifier ?? "unknown";

        // Push to receiver (personal group = their userId)
        await Clients.Group(receiverId).SendAsync("ReceiveDirectMessage", new
        {
            senderId,
            senderName,
            content,
            sentAt = DateTime.UtcNow
        });

        // Echo back to all sender tabs so they see their own message
        await Clients.Group(senderId).SendAsync("ReceiveDirectMessage", new
        {
            senderId,
            senderName,
            content,
            sentAt = DateTime.UtcNow
        });
    }

    // ── Group Chat ──────────────────────────────────────────────────────────
    /// <summary>
    /// Joins a SignalR group for real-time group chat.
    /// Call this after the page loads for every group the user belongs to.
    /// </summary>
    public async Task JoinGroupRoom(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
        await Clients.Group($"group_{groupId}")
            .SendAsync("UserJoinedGroup", Context.UserIdentifier, groupId);
    }

    public async Task LeaveGroupRoom(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
        await Clients.Group($"group_{groupId}")
            .SendAsync("UserLeftGroup", Context.UserIdentifier, groupId);
    }

    /// <summary>
    /// Broadcasts a group message to all members in the SignalR group.
    /// </summary>
    public async Task SendGroupMessage(string groupId, string senderName, string content)
    {
        var senderId = Context.UserIdentifier ?? "unknown";

        await Clients.Group($"group_{groupId}").SendAsync("ReceiveGroupMessage", new
        {
            groupId,
            senderId,
            senderName,
            content,
            sentAt = DateTime.UtcNow
        });
    }

    // ── Utility ─────────────────────────────────────────────────────────────
    public static bool IsUserOnline(string userId)
        => _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
}
