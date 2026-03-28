using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Infrastructure.Repositories
{
    /// <summary>
    /// Extends the shared Repository&lt;Group&gt; with group-chat-specific query methods.
    /// Never calls SaveChanges — that belongs to UnitOfWork.CompleteAsync().
    /// </summary>
    public class GroupChatRepository : Repository<Group>, IGroupChatRepository
    {
        // Repository<Group> already exposes _context and _dbSet (protected)
        public GroupChatRepository(AppDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<Group?> GetGroupWithMembersAsync(Guid groupId)
        {
            return await _dbSet
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g!.GroupMembers)
                .Select(gm => gm.Group!)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<GroupMember?> GetMemberAsync(Guid groupId, string userId)
        {
            return await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsMemberAsync(Guid groupId, string userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GroupMember>> GetAllMembersAsync(Guid groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AddMemberAsync(GroupMember member)
        {
            // No SaveChanges — caller must call UnitOfWork.CompleteAsync()
            await _context.GroupMembers.AddAsync(member);
        }

        /// <inheritdoc/>
        public void RemoveMember(GroupMember member)
        {
            // Synchronous — EF just marks it for deletion, UoW commits
            _context.GroupMembers.Remove(member);
        }

        /// <inheritdoc/>
        public async Task AddGroupMessageAsync(GroupMessages message)
        {
            // No SaveChanges — caller must call UnitOfWork.CompleteAsync()
            await _context.GroupMessages.AddAsync(message);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GroupMessages>> GetGroupHistoryAsync(
            Guid groupId, int page, int pageSize)
        {
            return await _context.GroupMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<GroupMessages?> GetGroupMessageByIdAsync(Guid messageId)
        {
            return await _context.GroupMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        /// <inheritdoc/>
        public async Task EditGroupMessageAsync(Guid messageId, string newContent)
        {
            await _context.GroupMessages
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.Content, newContent)
                    .SetProperty(m => m.IsEdited, true)
                    .SetProperty(m => m.EditedAt, DateTime.UtcNow)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }

        /// <inheritdoc/>
        public async Task DeleteGroupMessageAsync(Guid messageId)
        {
            await _context.GroupMessages
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.IsDeleted, true)
                    .SetProperty(m => m.DeletedAt, DateTime.UtcNow)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }
    }
}
