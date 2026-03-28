using SocialMedia.Application.DTOs.NotificationDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20, int skip = 0)
        {
            if (take <= 0) take = 20;
            if (take > 100) take = 100;
            if (skip < 0) skip = 0;

            var notifications = await _uow.Notifications.GetForUserAsync(userId, skip, take);

            return notifications
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Content = n.Content,
                    ActionUrl = BuildActionUrl(n.Type, n.SenderId),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    SenderId = n.SenderId,
                    SenderName = n.Sender != null ? (n.Sender.UserName ?? "Unknown") : "Unknown",
                    SenderProfilePictureUrl = n.Sender != null ? n.Sender.ProfilePictureUrl : null
                })
                .ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _uow.Notifications.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(string userId, Guid notificationId)
        {
            var notification = await _uow.Notifications.GetByIdForUserAsync(notificationId, userId);

            if (notification == null)
                return false;

            if (notification.IsRead)
                return true;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            _uow.Notifications.Update(notification);

            return await _uow.CompleteAsync() > 0;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _uow.Notifications.GetUnreadForUserAsync(userId);

            if (unreadNotifications.Count == 0)
                return 0;

            var now = DateTime.UtcNow;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = now;
                _uow.Notifications.Update(notification);
            }

            await _uow.CompleteAsync();
            return unreadNotifications.Count;
        }

        private static string BuildActionUrl(NotificationType type, string? senderId)
        {
            return type switch
            {
                NotificationType.Message when !string.IsNullOrWhiteSpace(senderId)
                    => $"/Messaging/Chat?otherUserId={senderId}",
                NotificationType.FriendRequest when !string.IsNullOrWhiteSpace(senderId)
                    => $"/Profile/Index?id={senderId}",
                NotificationType.PostReaction
                    => "/Home/Index",
                NotificationType.Comment
                    => "/Home/Index",
                NotificationType.GroupInvitation
                    => "/GroupChat/Index",
                _ => "/Home/Index"
            };
        }
    }
}
