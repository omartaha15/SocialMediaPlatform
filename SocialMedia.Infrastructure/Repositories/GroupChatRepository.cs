using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Infrastructure.Repositories
{
    public class GroupChatRepository : IGroupChatRepository
    {
        private readonly AppDbContext _context;

        public GroupChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Group> AddGroupAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> GetGroupByIdAsync(Guid groupId)
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Select(gm => gm.Group!)
                .ToListAsync();
        }

        public async Task<GroupMember?> GetMemberAsync(Guid groupId, string userId)
        {
            return await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        public async Task<bool> IsMemberAsync(Guid groupId, string userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        public async Task<IEnumerable<GroupMember>> GetAllMembersAsync(Guid groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .ToListAsync();
        }

        public async Task AddMemberAsync(GroupMember member)
        {
            await _context.GroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(GroupMember member)
        {
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }

        public async Task<GroupMessages> AddGroupMessageAsync(GroupMessages message)
        {
            await _context.GroupMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
