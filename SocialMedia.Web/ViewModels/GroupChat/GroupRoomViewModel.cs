using SocialMedia.Application.DTOs.GroupChatDTOs;

namespace SocialMedia.Web.ViewModels.GroupChat
{
    public class GroupRoomViewModel
    {
        public GroupDto Group { get; set; } = new();
        public List<GroupMessageDto> History { get; set; } = new();
        public List<GroupMemberDto> Members { get; set; } = new();
        public bool IsAdmin { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
    }
}
