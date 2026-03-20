using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Services
{
    /// <summary>
    /// Handles group chat business logic.
    /// Depends only on Application interfaces — no Infrastructure reference.
    /// </summary>
    public class GroupChatService : IGroupChatService
    {
        private readonly IGroupChatRepository _groupRepo;

        public GroupChatService(IGroupChatRepository groupRepo)
        {
            _groupRepo = groupRepo;
        }

        // ── Create Group ──────────────────────────────────────────────────────
        public async Task<GroupDto> CreateGroupAsync(string creatorId, CreateGroupDto dto)
        {
            var group = new Group
            {
                Name        = dto.Name,
                Description = dto.Description,
                CreatedAt   = DateTime.UtcNow
            };
            await _groupRepo.AddGroupAsync(group);

            await _groupRepo.AddMemberAsync(new GroupMember
            {
                GroupId   = group.Id,
                UserId    = creatorId,
                Role      = GroupRole.Admin,
                CreatedAt = DateTime.UtcNow
            });

            return MapToGroupDto(group, 1);
        }

        // ── Get Group ─────────────────────────────────────────────────────────
        public async Task<GroupDto?> GetGroupByIdAsync(Guid groupId)
        {
            var group = await _groupRepo.GetGroupByIdAsync(groupId);
            if (group == null) return null;
            var members = await _groupRepo.GetAllMembersAsync(groupId);
            return MapToGroupDto(group, members.Count());
        }

        // ── Get User Groups ───────────────────────────────────────────────────
        public async Task<IEnumerable<GroupDto>> GetUserGroupsAsync(string userId)
        {
            var groups = await _groupRepo.GetGroupsByUserIdAsync(userId);
            var tasks  = groups.Select(async g =>
            {
                var members = await _groupRepo.GetAllMembersAsync(g.Id);
                return MapToGroupDto(g, members.Count());
            });
            return await Task.WhenAll(tasks);
        }

        // ── Add Member ────────────────────────────────────────────────────────
        public async Task AddMemberAsync(Guid groupId, string requesterId, string targetUserId)
        {
            var requester = await _groupRepo.GetMemberAsync(groupId, requesterId);
            if (requester?.Role != GroupRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add members.");

            if (await _groupRepo.IsMemberAsync(groupId, targetUserId))
                throw new InvalidOperationException("User is already a member.");

            await _groupRepo.AddMemberAsync(new GroupMember
            {
                GroupId   = groupId,
                UserId    = targetUserId,
                Role      = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            });
        }

        // ── Remove Member ─────────────────────────────────────────────────────
        public async Task RemoveMemberAsync(Guid groupId, string requesterId, string targetUserId)
        {
            var requester = await _groupRepo.GetMemberAsync(groupId, requesterId);
            if (requester?.Role != GroupRole.Admin)
                throw new UnauthorizedAccessException("Only admins can remove members.");

            var target = await _groupRepo.GetMemberAsync(groupId, targetUserId)
                ?? throw new InvalidOperationException("User is not a member.");

            await _groupRepo.RemoveMemberAsync(target);
        }

        // ── Leave Group ───────────────────────────────────────────────────────
        public async Task LeaveGroupAsync(Guid groupId, string userId)
        {
            var member = await _groupRepo.GetMemberAsync(groupId, userId)
                ?? throw new InvalidOperationException("You are not a member of this group.");
            await _groupRepo.RemoveMemberAsync(member);
        }

        // ── Get Members ───────────────────────────────────────────────────────
        public async Task<IEnumerable<GroupMemberDto>> GetMembersAsync(Guid groupId)
        {
            var members = await _groupRepo.GetAllMembersAsync(groupId);
            return members.Select(gm => new GroupMemberDto
            {
                UserId         = gm.UserId,
                UserName       = gm.User?.UserName ?? "Unknown",
                ProfilePicture = gm.User?.ProfilePictureUrl,
                Role           = gm.Role.ToString()
            });
        }

        // ── Send Group Message ────────────────────────────────────────────────
        public async Task<GroupMessageDto> SendGroupMessageAsync(string senderId, SendGroupMessageDto dto)
        {
            if (!await _groupRepo.IsMemberAsync(dto.GroupId, senderId))
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var msg = new GroupMessages
            {
                GroupId   = dto.GroupId,
                SenderId  = senderId,
                Content   = dto.Content,
                CreatedAt = DateTime.UtcNow
            };
            await _groupRepo.AddGroupMessageAsync(msg);

            return new GroupMessageDto
            {
                Id                   = msg.Id,
                Content              = msg.Content,
                CreatedAt            = msg.CreatedAt,
                SenderId             = msg.SenderId,
                SenderName           = msg.Sender?.UserName ?? "Unknown",
                SenderProfilePicture = msg.Sender?.ProfilePictureUrl,
                GroupId              = msg.GroupId
            };
        }

        // ── Group History ─────────────────────────────────────────────────────
        public async Task<IEnumerable<GroupMessageDto>> GetGroupHistoryAsync(
            Guid groupId, int page = 1, int pageSize = 20)
        {
            var messages = await _groupRepo.GetGroupHistoryAsync(groupId, page, pageSize);
            return messages.Select(m => new GroupMessageDto
            {
                Id                   = m.Id,
                Content              = m.Content,
                CreatedAt            = m.CreatedAt,
                SenderId             = m.SenderId,
                SenderName           = m.Sender?.UserName ?? "Unknown",
                SenderProfilePicture = m.Sender?.ProfilePictureUrl,
                GroupId              = m.GroupId
            });
        }

        // ── Is Member ─────────────────────────────────────────────────────────
        public async Task<bool> IsGroupMemberAsync(Guid groupId, string userId)
            => await _groupRepo.IsMemberAsync(groupId, userId);

        // ── Mapper ────────────────────────────────────────────────────────────
        private static GroupDto MapToGroupDto(Group g, int memberCount) => new()
        {
            Id          = g.Id,
            Name        = g.Name,
            Description = g.Description,
            ImageUrl    = g.ImageUrl,
            MemberCount = memberCount,
            CreatedAt   = g.CreatedAt
        };
    }
}
