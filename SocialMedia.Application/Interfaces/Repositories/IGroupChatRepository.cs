using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces.Repositories
{
    /// <summary>
    /// Custom query methods for group chat.
    /// Extends the generic repository — SaveChanges is handled by IUnitOfWork.CompleteAsync().
    /// </summary>
    public interface IGroupChatRepository : IGenericRepository<Group>
    {
        Task<Group?> GetGroupWithMembersAsync(Guid groupId);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(string userId);
        Task<GroupMember?> GetMemberAsync(Guid groupId, string userId);
        Task<bool> IsMemberAsync(Guid groupId, string userId);
        Task<IEnumerable<GroupMember>> GetAllMembersAsync(Guid groupId);
        Task AddMemberAsync(GroupMember member);
        void RemoveMember(GroupMember member);
        Task AddGroupMessageAsync(GroupMessages message);
        Task<IEnumerable<GroupMessages>> GetGroupHistoryAsync(Guid groupId, int page, int pageSize);
        
        /// <summary>Gets a single group message by ID with sender info.</summary>
        Task<GroupMessages?> GetGroupMessageByIdAsync(Guid messageId);

        /// <summary>Edits a group message's content and marks it as edited.</summary>
        Task EditGroupMessageAsync(Guid messageId, string newContent);

        /// <summary>Soft deletes a group message by marking it as deleted.</summary>
        Task DeleteGroupMessageAsync(Guid messageId);
    }
}
