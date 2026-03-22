namespace SocialMedia.Application.DTOs.GroupChatDTOs
{
    public class GroupMessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfilePicture { get; set; }
        public Guid GroupId { get; set; }
    }
}
