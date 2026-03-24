using SocialMedia.Application.DTOs.ChatDTOs;

namespace SocialMedia.Web.ViewModels.Chat
{
    public class ConversationsViewModel
    {
        public List<ConversationSummaryDto> Conversations { get; set; } = new();
        public int TotalUnread { get; set; }
    }
}
