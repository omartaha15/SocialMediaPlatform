using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.PostDtos
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public int ReactionCount { get; set; }
        public string UserName { get; set; }
        public string userId { get; set; }
        public string? UserAvatarUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Count per reaction type for this post.</summary>
        public Dictionary<MultiReaction, int> ReactionCounts { get; set; } = new();

        /// <summary>The logged-in user's reaction on this post (null = no reaction).</summary>
        public MultiReaction? CurrentUserReaction { get; set; }
    }
}
