namespace SocialMedia.Application.DTOs.Search
{
    public class UserSearchDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? ProfileImage { get; set; }
    }
}