using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Custom repository for group chat queries.
    /// Defined in Application, implemented in Infrastructure.
    /// </summary>
    public interface IGroupChatRepository
    {
        Task<Group> AddGroupAsync(Group group);
        Task<Group?> GetGroupByIdAsync(Guid groupId);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId);
        Task<GroupMember?> GetMemberAsync(Guid groupId, string userId);
        Task<bool> IsMemberAsync(Guid groupId, string userId);
        Task<IEnumerable<GroupMember>> GetAllMembersAsync(Guid groupId);
        Task AddMemberAsync(GroupMember member);
        Task RemoveMemberAsync(GroupMember member);
        Task<GroupMessages> AddGroupMessageAsync(GroupMessages message);
        Task<IEnumerable<GroupMessages>> GetGroupHistoryAsync(Guid groupId, int page, int pageSize);
        Task SaveChangesAsync();
    }
}
