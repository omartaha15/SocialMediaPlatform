using SocialMedia.Application.DTOs.ChatDTOs;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto);
        Task<IEnumerable<MessageDto>> GetChatHistoryAsync(string userId1, string userId2, int page = 1, int pageSize = 20);
        Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(string userId);
        Task MarkAsReadAsync(string currentUserId, string otherUserId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
