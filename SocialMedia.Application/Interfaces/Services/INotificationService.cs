using SocialMedia.Application.DTOs.NotificationDTOs;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20);
    }
}