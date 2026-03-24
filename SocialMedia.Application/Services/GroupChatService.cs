using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Services
{
    /// <summary>
    /// Group chat business logic.
    /// Uses IUnitOfWork as the single entry point — no direct repo or EF references.
    /// </summary>
    public class GroupChatService : IGroupChatService
    {
        private readonly IUnitOfWork _uow;

        public GroupChatService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ── Create Group ──────────────────────────────────────────────────────
        public async Task<GroupDto> CreateGroupAsync(string creatorId, CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GroupChats.AddAsync(group);

            await _uow.GroupChats.AddMemberAsync(new GroupMember
            {
                GroupId = group.Id,
                UserId = creatorId,
                Role = GroupRole.Admin,
                CreatedAt = DateTime.UtcNow
            });

            await _uow.CompleteAsync();     // group + creator member in one transaction

            return MapToGroupDto(group, 1);
        }

        // ── Get Group ─────────────────────────────────────────────────────────
        public async Task<GroupDto?> GetGroupByIdAsync(Guid groupId)
        {
            var group = await _uow.GroupChats.GetGroupWithMembersAsync(groupId);
            if (group == null) return null;
            return MapToGroupDto(group, group.GroupMembers.Count);
        }

        // ── Get User Groups ───────────────────────────────────────────────────
        public async Task<IEnumerable<GroupDto>> GetUserGroupsAsync(string userId)
        {
            var groups = await _uow.GroupChats.GetGroupsByUserIdAsync(userId);
            return groups.Select(g => MapToGroupDto(g, g.GroupMembers.Count));
        }

        // ── Add Member ────────────────────────────────────────────────────────
        public async Task AddMemberAsync(Guid groupId, string requesterId, string targetUserId)
        {
            var requester = await _uow.GroupChats.GetMemberAsync(groupId, requesterId);
            if (requester?.Role != GroupRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add members.");

            if (await _uow.GroupChats.IsMemberAsync(groupId, targetUserId))
                throw new InvalidOperationException("User is already a member.");

            await _uow.GroupChats.AddMemberAsync(new GroupMember
            {
                GroupId = groupId,
                UserId = targetUserId,
                Role = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            });

            await _uow.CompleteAsync();
        }

        // ── Remove Member ─────────────────────────────────────────────────────
        public async Task RemoveMemberAsync(Guid groupId, string requesterId, string targetUserId)
        {
            var requester = await _uow.GroupChats.GetMemberAsync(groupId, requesterId);
            if (requester?.Role != GroupRole.Admin)
                throw new UnauthorizedAccessException("Only admins can remove members.");

            var target = await _uow.GroupChats.GetMemberAsync(groupId, targetUserId)
                ?? throw new InvalidOperationException("User is not a member.");

            _uow.GroupChats.RemoveMember(target);
            await _uow.CompleteAsync();
        }

        // ── Leave Group ───────────────────────────────────────────────────────
        public async Task LeaveGroupAsync(Guid groupId, string userId)
        {
            var member = await _uow.GroupChats.GetMemberAsync(groupId, userId)
                ?? throw new InvalidOperationException("You are not a member of this group.");

            _uow.GroupChats.RemoveMember(member);
            await _uow.CompleteAsync();
        }

        // ── Get Members ───────────────────────────────────────────────────────
        public async Task<IEnumerable<GroupMemberDto>> GetMembersAsync(Guid groupId)
        {
            var members = await _uow.GroupChats.GetAllMembersAsync(groupId);
            return members.Select(gm => new GroupMemberDto
            {
                UserId = gm.UserId,
                UserName = gm.User?.UserName ?? "Unknown",
                ProfilePicture = gm.User?.ProfilePictureUrl,
                Role = gm.Role.ToString()
            });
        }

        // ── Send Group Message ────────────────────────────────────────────────
        public async Task<GroupMessageDto> SendGroupMessageAsync(string senderId, SendGroupMessageDto dto)
        {
            if (!await _uow.GroupChats.IsMemberAsync(dto.GroupId, senderId))
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var msg = new GroupMessages
            {
                GroupId = dto.GroupId,
                SenderId = senderId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GroupChats.AddGroupMessageAsync(msg);
            await _uow.CompleteAsync();

            // ApplicationUser is not a BaseEntity so we use FindUserByIdAsync, not Repository<T>
            var sender = await _uow.FindUserByIdAsync(senderId);

            return new GroupMessageDto
            {
                Id = msg.Id,
                Content = msg.Content,
                CreatedAt = msg.CreatedAt,
                SenderId = msg.SenderId,
                SenderName = sender?.UserName ?? "Unknown",
                SenderProfilePicture = sender?.ProfilePictureUrl,
                GroupId = msg.GroupId
            };
        }

        // ── Group History ─────────────────────────────────────────────────────
        public async Task<IEnumerable<GroupMessageDto>> GetGroupHistoryAsync(
            Guid groupId, int page = 1, int pageSize = 20)
        {
            var messages = await _uow.GroupChats
                .GetGroupHistoryAsync(groupId, page, pageSize);

            return messages.Select(m => new GroupMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                SenderId = m.SenderId,
                SenderName = m.Sender?.UserName ?? "Unknown",
                SenderProfilePicture = m.Sender?.ProfilePictureUrl,
                GroupId = m.GroupId
            });
        }

        // ── Is Member ─────────────────────────────────────────────────────────
        public async Task<bool> IsGroupMemberAsync(Guid groupId, string userId)
            => await _uow.GroupChats.IsMemberAsync(groupId, userId);

        // ── Mapper ────────────────────────────────────────────────────────────
        private static GroupDto MapToGroupDto(Group g, int memberCount) => new()
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            ImageUrl = g.ImageUrl,
            MemberCount = memberCount,
            CreatedAt = g.CreatedAt
        };
    }
}
