using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.DTOs.ProfileDTOs;

namespace SocialMedia.Web.ViewModels.GroupChat
{
    public class GroupRoomViewModel
    {
        public GroupDto Group { get; set; } = new();
        public List<GroupMessageDto> History { get; set; } = new();
        public List<GroupMemberDto> Members { get; set; } = new();
        /// <summary>Friends not yet in the group — shown in the Add Member dropdown (admin only).</summary>
        public List<UserSuggestionDto> AvailableFriendsToAdd { get; set; } = new();
        public bool IsAdmin { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
    }
}
