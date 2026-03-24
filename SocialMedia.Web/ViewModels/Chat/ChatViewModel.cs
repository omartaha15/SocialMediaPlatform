using SocialMedia.Application.DTOs.ChatDTOs;

namespace SocialMedia.Web.ViewModels.Chat
{
    public class ChatViewModel
    {
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserProfilePicture { get; set; }
        public List<MessageDto> History { get; set; } = new();
    }
}
