namespace SocialMedia.Application.DTOs.GroupChatDTOs
{
    public class GroupMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
