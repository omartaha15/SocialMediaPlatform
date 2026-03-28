namespace SocialMedia.Application.DTOs.ChatDTOs
{

    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfilePicture { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
    }
}
