using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Enums;
using SocialMedia.Web.Hubs;

namespace SocialMedia.Web.Services
{
    public class NotificationRealtimeService : INotificationRealtimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRealtimeService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushAsync(
            string targetUserId,
            NotificationType type,
            string message,
            string? actionUrl = null,
            Guid? notificationId = null,
            DateTime? createdAt = null)
        {
            await _hubContext.Clients.Group(targetUserId).SendAsync("ReceiveNotification", new
            {
                id = notificationId,
                type = type.ToString(),
                message,
                actionUrl,
                createdAt = createdAt ?? DateTime.UtcNow
            });
        }
    }
}
