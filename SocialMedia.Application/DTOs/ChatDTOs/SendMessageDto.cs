namespace SocialMedia.Application.DTOs.ChatDTOs
{
    public class SendMessageDto
    {
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
