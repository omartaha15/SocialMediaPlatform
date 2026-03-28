using SocialMedia.Application.DTOs.GroupChatDTOs;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IGroupChatService
    {
        Task<GroupDto> CreateGroupAsync(string creatorId, CreateGroupDto dto);
        Task<GroupDto?> GetGroupByIdAsync(Guid groupId);
        Task<IEnumerable<GroupDto>> GetUserGroupsAsync(string userId);
        Task AddMemberAsync(Guid groupId, string requesterId, string targetUserId);
        Task RemoveMemberAsync(Guid groupId, string requesterId, string targetUserId);
        Task LeaveGroupAsync(Guid groupId, string userId);
        Task<IEnumerable<GroupMemberDto>> GetMembersAsync(Guid groupId);
        Task<GroupMessageDto> SendGroupMessageAsync(string senderId, SendGroupMessageDto dto);
        Task<IEnumerable<GroupMessageDto>> GetGroupHistoryAsync(Guid groupId, int page = 1, int pageSize = 20);
        Task<bool> IsGroupMemberAsync(Guid groupId, string userId);
        Task<GroupMessageDto> EditGroupMessageAsync(Guid messageId, string userId, string newContent);
        Task DeleteGroupMessageAsync(Guid messageId, string userId);
    }
}
