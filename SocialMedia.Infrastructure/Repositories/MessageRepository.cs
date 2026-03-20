using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Message> AddMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Reload with Sender so the caller gets a fully-populated entity
            await _context.Entry(message)
                .Reference(m => m.Sender)
                .LoadAsync();

            return message;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatHistoryAsync(
            string userId1, string userId2, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                    (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)   // re-order chunk oldest→newest for display
                .ToListAsync();
        }

        /// <inheritdoc/>
        /// Uses a GROUP BY in SQL to fetch only one row per conversation partner —
        /// avoids loading all messages into memory.
        public async Task<IEnumerable<Message>> GetLatestMessagePerConversationAsync(string userId)
        {
            // Get the Id of the latest message per (userId, partner) pair
            var latestIds = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.CreatedAt).First().Id)
                .ToListAsync();

            // Fetch those specific messages with their navigation properties
            return await _context.Messages
                .Where(m => latestIds.Contains(m.Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        /// Mutation stays in the repo — service never touches EF-tracked state directly.
        public async Task MarkAsReadAsync(string currentUserId, string otherUserId)
        {
            await _context.Messages
                .Where(m => m.SenderId == otherUserId
                         && m.ReceiverId == currentUserId
                         && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
