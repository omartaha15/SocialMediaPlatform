using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetForUserAsync(string userId, int skip, int take);
        Task<int> GetUnreadCountAsync(string userId);
        Task<Notification?> GetByIdForUserAsync(Guid notificationId, string userId);
        Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(string userId);
    }
}
