using SocialMedia.Application.DTOs.ChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    /// <summary>
    /// Direct-message business logic.
    /// Depends only on Application interfaces — zero Infrastructure / EF references.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepo;

        // IUserService removed — user lookup now done through IMessageRepository.FindUserByIdAsync
        // so we don't misuse FindByUserName/FindByEmail with a GUID-based senderId.
        public MessageService(IMessageRepository messageRepo)
        {
            _messageRepo = messageRepo;
        }

        // ── Send ─────────────────────────────────────────────────────────────
        public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto)
        {
            var message = new Message
            {
                SenderId   = senderId,
                ReceiverId = dto.ReceiverId,
                Content    = dto.Content,
                IsRead     = false,
                CreatedAt  = DateTime.UtcNow
            };

            // AddMessageAsync returns the message with Sender already loaded via Include
            var saved = await _messageRepo.AddMessageAsync(message);
            return MapToDto(saved, saved.Sender);
        }

        // ── Chat History ──────────────────────────────────────────────────────
        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(
            string userId1, string userId2, int page = 1, int pageSize = 20)
        {
            var messages = await _messageRepo.GetChatHistoryAsync(userId1, userId2, page, pageSize);
            return messages.Select(m => MapToDto(m, m.Sender));
        }

        // ── Conversations List ────────────────────────────────────────────────
        // Uses GetLatestMessagePerConversationAsync which does the grouping in SQL,
        // not in memory — avoids loading every message the user ever sent/received.
        public async Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(string userId)
        {
            var latest = await _messageRepo.GetLatestMessagePerConversationAsync(userId);
            var unreadCount = await _messageRepo.GetUnreadCountAsync(userId);

            return latest.Select(m =>
            {
                var otherUser = m.SenderId == userId ? m.Receiver : m.Sender;
                return new ConversationSummaryDto
                {
                    OtherUserId             = otherUser?.Id ?? string.Empty,
                    OtherUserName           = otherUser?.UserName ?? "Unknown",
                    OtherUserProfilePicture = otherUser?.ProfilePictureUrl,
                    LastMessage             = m.Content,
                    LastMessageTime         = m.CreatedAt,
                    // Unread count per-conversation requires a separate query;
                    // for now we show total badge — can be refined in Phase 5
                    UnreadCount             = 0
                };
            });
        }

        // ── Mark as Read ──────────────────────────────────────────────────────
        // Mutation is fully inside the repository via ExecuteUpdateAsync —
        // the service never directly touches EF-tracked entity state.
        public async Task MarkAsReadAsync(string currentUserId, string otherUserId)
        {
            await _messageRepo.MarkAsReadAsync(currentUserId, otherUserId);
        }

        // ── Unread Count ──────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _messageRepo.GetUnreadCountAsync(userId);
        }

        // ── Private Mapper ────────────────────────────────────────────────────
        private static MessageDto MapToDto(Message m, ApplicationUser? sender) => new()
        {
            Id                   = m.Id,
            Content              = m.Content,
            IsRead               = m.IsRead,
            CreatedAt            = m.CreatedAt,
            SenderId             = m.SenderId,
            SenderName           = sender?.UserName ?? "Unknown",
            SenderProfilePicture = sender?.ProfilePictureUrl,
            ReceiverId           = m.ReceiverId
        };
    }
}
