namespace SocialMedia.Application.DTOs.GroupChatDTOs
{
    public class SendGroupMessageDto
    {
        public Guid GroupId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
