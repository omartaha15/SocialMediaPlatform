using SocialMedia.Application.DTOs.ChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    /// <summary>
    /// Direct-message business logic.
    /// Uses IUnitOfWork as the single entry point — no direct repo or EF references.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;

        public MessageService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ── Send ──────────────────────────────────────────────────────────────
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

            await _uow.Messages.AddAsync(message);
            await _uow.CompleteAsync();

            // ApplicationUser is not a BaseEntity so we use FindUserByIdAsync, not Repository<T>
            var sender = await _uow.FindUserByIdAsync(senderId);

            return MapToDto(message, sender);
        }

        // ── Chat History ──────────────────────────────────────────────────────
        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(
            string userId1, string userId2, int page = 1, int pageSize = 20)
        {
            var messages = await _uow.Messages
                .GetChatHistoryAsync(userId1, userId2, page, pageSize);

            return messages.Select(m => MapToDto(m, m.Sender));
        }

        // ── Conversations List ────────────────────────────────────────────────
        public async Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(string userId)
        {
            var latest = await _uow.Messages
                .GetLatestMessagePerConversationAsync(userId);

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
                    UnreadCount             = 0   // refined in Phase 5
                };
            });
        }

        // ── Mark as Read ──────────────────────────────────────────────────────
        public async Task MarkAsReadAsync(string currentUserId, string otherUserId)
        {
            await _uow.Messages.MarkAsReadAsync(currentUserId, otherUserId);
        }

        // ── Unread Count ──────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _uow.Messages.GetUnreadCountAsync(userId);
        }

        // ── Mapper ────────────────────────────────────────────────────────────
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
