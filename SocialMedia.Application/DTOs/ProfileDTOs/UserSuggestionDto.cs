namespace SocialMedia.Application.DTOs.ProfileDTOs
{
    public class UserSuggestionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string? Location { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
