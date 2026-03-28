using Microsoft.Extensions.Logging;
using SocialMedia.Application.DTOs.ChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Services
{
    /// <summary>
    /// Direct-message business logic.
    /// Uses IUnitOfWork as the single entry point — no direct repo or EF references.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationRealtimeService _notificationRealtimeService;
        private readonly ILogger<MessageService> _logger;
        private readonly IDashboardNotifierService _dashboardNotifier;
        public MessageService(
            IUnitOfWork uow,
            INotificationRealtimeService notificationRealtimeService,
            ILogger<MessageService> logger,
            IDashboardNotifierService dashboardNotifier)
        {
            _uow = uow;
            _notificationRealtimeService = notificationRealtimeService;
            _logger = logger;
            _dashboardNotifier = dashboardNotifier;
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

            // ApplicationUser is not a BaseEntity so we use FindUserByIdAsync, not Repository<T>
            var sender = await _uow.FindUserByIdAsync(senderId);
            var senderName = sender?.UserName ?? "Someone";
            var notificationMessage = $"{senderName} sent you a message.";
            Notification? notification = null;

            if (senderId != dto.ReceiverId)
            {
                notification = new Notification
                {
                    UserId = dto.ReceiverId,
                    SenderId = senderId,
                    Type = NotificationType.Message,
                    Content = notificationMessage
                };

                await _uow.Repository<Notification>().AddAsync(notification);
            }

            await _uow.CompleteAsync();
            await _dashboardNotifier.NotifyDashboardUpdatedAsync();
            if (senderId != dto.ReceiverId)
            {
                try
                {
                    var actionUrl = $"/Messaging/Chat?otherUserId={senderId}";
                    await _notificationRealtimeService.PushAsync(
                        dto.ReceiverId,
                        NotificationType.Message,
                        notificationMessage,
                        actionUrl: actionUrl,
                        notificationId: notification?.Id,
                        createdAt: notification?.CreatedAt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to push realtime message notification. TargetUserId: {TargetUserId}",
                        dto.ReceiverId);
                }
            }

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

            // Get per-sender unread counts in a single DB round-trip
            var unreadPerSender = await _uow.Messages
                .GetUnreadCountPerSenderAsync(userId);

            return latest.Select(m =>
            {
                var otherUser = m.SenderId == userId ? m.Receiver : m.Sender;
                var otherId   = otherUser?.Id ?? string.Empty;

                return new ConversationSummaryDto
                {
                    OtherUserId             = otherId,
                    OtherUserName           = otherUser?.UserName ?? "Unknown",
                    OtherUserProfilePicture = otherUser?.ProfilePictureUrl,
                    LastMessage             = m.Content,
                    LastMessageTime         = m.CreatedAt,
                    UnreadCount             = unreadPerSender.TryGetValue(otherId, out var cnt) ? cnt : 0
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

        // ── Get Message By Id ─────────────────────────────────────────────────
        public async Task<MessageDto?> GetMessageByIdAsync(Guid messageId)
        {
            var message = await _uow.Messages.GetMessageByIdAsync(messageId);
            if (message == null) return null;
            return MapToDto(message, message.Sender);
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private static MessageDto MapToDto(Message m, ApplicationUser? sender) => new()
        {
            Id                   = m.Id,
            Content              = m.Content,
            IsRead               = m.IsRead,
            IsEdited             = m.IsEdited,
            EditedAt             = m.EditedAt,
            IsDeleted            = m.IsDeleted,
            CreatedAt            = m.CreatedAt,
            SenderId             = m.SenderId,
            SenderName           = sender?.UserName ?? "Unknown",
            SenderProfilePicture = sender?.ProfilePictureUrl,
            ReceiverId           = m.ReceiverId
        };

        // ── Edit Message ──────────────────────────────────────────────────────
        public async Task<MessageDto> EditMessageAsync(Guid messageId, string userId, string newContent)
        {
            var message = await _uow.Messages.GetMessageByIdAsync(messageId);
            
            if (message == null)
                throw new InvalidOperationException("Message not found.");
            
            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only edit your own messages.");
            
            if (message.IsDeleted)
                throw new InvalidOperationException("Cannot edit a deleted message.");

            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Message content cannot be empty.");

            await _uow.Messages.EditMessageAsync(messageId, newContent.Trim());
            
            // Fetch updated message to return
            var updatedMessage = await _uow.Messages.GetMessageByIdAsync(messageId);
            return MapToDto(updatedMessage!, updatedMessage!.Sender);
        }

        // ── Delete Message ────────────────────────────────────────────────────
        public async Task<MessageDto> DeleteMessageAsync(Guid messageId, string userId)
        {
            var message = await _uow.Messages.GetMessageByIdAsync(messageId);
            
            if (message == null)
                throw new InvalidOperationException("Message not found.");
            
            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only delete your own messages.");
            
            if (message.IsDeleted)
                throw new InvalidOperationException("Message is already deleted.");

            // Store the message info before deletion
            var messageDto = MapToDto(message, message.Sender);

            await _uow.Messages.DeleteMessageAsync(messageId);
            await _dashboardNotifier.NotifyDashboardUpdatedAsync();
            return messageDto;
        }
    }
}
