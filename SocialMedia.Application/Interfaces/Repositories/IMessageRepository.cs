using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces.Repositories
{
    /// <summary>
    /// Custom query methods for direct messages.
    /// Extends the generic repository — SaveChanges is handled by IUnitOfWork.CompleteAsync().
    /// </summary>
    public interface IMessageRepository : IGenericRepository<Message>
    {
        /// <summary>Paginated chat history between two users, ordered oldest→newest.</summary>
        Task<IEnumerable<Message>> GetChatHistoryAsync(string userId1, string userId2, int page, int pageSize);

        /// <summary>
        /// Returns the latest message per conversation partner for the given user.
        /// Includes Sender and Receiver navigation properties.
        /// </summary>
        Task<IEnumerable<Message>> GetLatestMessagePerConversationAsync(string userId);

        /// <summary>Bulk-marks messages from otherUserId to currentUserId as read.</summary>
        Task MarkAsReadAsync(string currentUserId, string otherUserId);

        /// <summary>Total count of unread messages received by userId.</summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Returns a dictionary of senderId -> unread count for all unread messages
        /// received by userId. Used to show per-conversation unread badges.
        /// </summary>
        Task<Dictionary<string, int>> GetUnreadCountPerSenderAsync(string userId);
    }
}
