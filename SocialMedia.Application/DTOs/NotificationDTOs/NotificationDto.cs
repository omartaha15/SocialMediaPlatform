using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.DTOs.NotificationDTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SenderId { get; set; }
        public string SenderName { get; set; } = "Unknown";
        public string? SenderProfilePictureUrl { get; set; }
    }
}