using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.NotificationDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20)
        {
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var notifications = await _uow.Repository<Notification>()
                .Query()
                .Where(n => n.UserId == userId)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    SenderId = n.SenderId,
                    SenderName = n.Sender != null ? (n.Sender.UserName ?? "Unknown") : "Unknown",
                    SenderProfilePictureUrl = n.Sender != null ? n.Sender.ProfilePictureUrl : null
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(string userId, Guid notificationId)
        {
            var notification = await _uow.Repository<Notification>()
                .Query()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            if (notification.IsRead)
                return true;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            _uow.Repository<Notification>().Update(notification);

            return await _uow.CompleteAsync() > 0;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _uow.Repository<Notification>()
                .Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Count == 0)
                return 0;

            var now = DateTime.UtcNow;
            var repo = _uow.Repository<Notification>();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = now;
                repo.Update(notification);
            }

            await _uow.CompleteAsync();
            return unreadNotifications.Count;
        }
    }
}
