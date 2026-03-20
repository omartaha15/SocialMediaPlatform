using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Custom repository for direct message queries.
    /// Defined in Application, implemented in Infrastructure.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>Persist a new message and return it with Sender navigation loaded.</summary>
        Task<Message> AddMessageAsync(Message message);

        /// <summary>Paginated chat history between two users, ordered oldest→newest.</summary>
        Task<IEnumerable<Message>> GetChatHistoryAsync(string userId1, string userId2, int page, int pageSize);

        /// <summary>
        /// Returns the latest message per conversation partner for the given user.
        /// Includes Sender and Receiver navigation properties.
        /// </summary>
        Task<IEnumerable<Message>> GetLatestMessagePerConversationAsync(string userId);

        /// <summary>Returns all unread messages sent by otherUserId to currentUserId.</summary>
        Task MarkAsReadAsync(string currentUserId, string otherUserId);

        /// <summary>Total count of unread messages for a user.</summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>Look up a user by their Id (GUID string) — avoids depending on IUserService.</summary>
        Task<ApplicationUser?> FindUserByIdAsync(string userId);
    }
}
