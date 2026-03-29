using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.DTOs.PostDtos.ReactionDtos
{
    public class ReactorDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public MultiReaction ReactionType { get; set; }
    }
}
