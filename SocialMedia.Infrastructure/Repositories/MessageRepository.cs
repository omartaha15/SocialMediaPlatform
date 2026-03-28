using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Infrastructure.Repositories
{
    /// <summary>
    /// Extends the shared Repository&lt;Message&gt; with chat-specific query methods.
    /// Never calls SaveChanges — that belongs to UnitOfWork.CompleteAsync().
    /// </summary>
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        // Repository<Message> already exposes _context and _dbSet (protected)
        public MessageRepository(AppDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatHistoryAsync(
            string userId1, string userId2, int page, int pageSize)
        {
            return await _dbSet
                .Where(m =>
                    (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                    (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)  // re-sort chunk oldest→newest for display
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetLatestMessagePerConversationAsync(string userId)
        {
            // Step 1 — get the Id of the latest message per conversation partner (done in SQL)
            var latestIds = await _dbSet
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.CreatedAt).First().Id)
                .ToListAsync();

            // Step 2 — fetch those rows with navigations loaded
            return await _dbSet
                .Where(m => latestIds.Contains(m.Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task MarkAsReadAsync(string currentUserId, string otherUserId)
        {
            // Bulk UPDATE in one SQL statement — no entity loading, no SaveChanges needed
            await _dbSet
                .Where(m => m.SenderId == otherUserId
                         && m.ReceiverId == currentUserId
                         && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _dbSet
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, int>> GetUnreadCountPerSenderAsync(string userId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SenderId, x => x.Count);
        }

        /// <inheritdoc/>
        public async Task<Message?> GetMessageByIdAsync(Guid messageId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        /// <inheritdoc/>
        public async Task EditMessageAsync(Guid messageId, string newContent)
        {
            await _dbSet
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.Content, newContent)
                    .SetProperty(m => m.IsEdited, true)
                    .SetProperty(m => m.EditedAt, DateTime.UtcNow)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }

        /// <inheritdoc/>
        public async Task DeleteMessageAsync(Guid messageId)
        {
            await _dbSet
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.IsDeleted, true)
                    .SetProperty(m => m.DeletedAt, DateTime.UtcNow)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }
    }
}
