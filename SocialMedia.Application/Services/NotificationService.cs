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
    }
}
