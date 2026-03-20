using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.ChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;

        public MessageService(AppDbContext context)
        {
            _context = context;
        }

        // ── Send ────────────────────────────────────────────────────────────
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

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId);
            return new MessageDto
            {
                Id                   = message.Id,
                Content              = message.Content,
                IsRead               = message.IsRead,
                CreatedAt            = message.CreatedAt,
                SenderId             = message.SenderId,
                SenderName           = sender?.UserName ?? "Unknown",
                SenderProfilePicture = sender?.ProfilePictureUrl,
                ReceiverId           = message.ReceiverId
            };
        }

        // ── Chat History ────────────────────────────────────────────────────
        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(
            string userId1, string userId2, int page = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                    (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id                   = m.Id,
                    Content              = m.Content,
                    IsRead               = m.IsRead,
                    CreatedAt            = m.CreatedAt,
                    SenderId             = m.SenderId,

                    SenderName           = m.Sender != null ? m.Sender.UserName ?? "" : "",
                    SenderProfilePicture = m.Sender != null ? m.Sender.ProfilePictureUrl : null,
                    ReceiverId           = m.ReceiverId
                })
                .ToListAsync();
        }

        // ── Conversations List ──────────────────────────────────────────────
        public async Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g =>
                {
                    var last      = g.First();
                    var otherUser = last.SenderId == userId ? last.Receiver : last.Sender;
                    return new ConversationSummaryDto
                    {
                        OtherUserId             = g.Key,
                        OtherUserName           = otherUser?.UserName ?? "Unknown",
                        OtherUserProfilePicture = otherUser?.ProfilePictureUrl,
                        LastMessage             = last.Content,
                        LastMessageTime         = last.CreatedAt,
                        UnreadCount             = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                    };
                }).ToList();
        }

        // ── Mark as Read ────────────────────────────────────────────────────
        public async Task MarkAsReadAsync(string currentUserId, string otherUserId)
        {
            var unread = await _context.Messages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }

        // ── Unread Count ────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }
}
