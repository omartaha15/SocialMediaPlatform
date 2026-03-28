using SocialMedia.Application.DTOs.NotificationDTOs;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20, int skip = 0);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string userId, Guid notificationId);
        Task<int> MarkAllAsReadAsync(string userId);
    }
}