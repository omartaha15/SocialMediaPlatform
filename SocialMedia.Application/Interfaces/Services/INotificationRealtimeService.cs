using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface INotificationRealtimeService
    {
        Task PushAsync(string targetUserId, NotificationType type, string message, string? actionUrl = null);
    }
}