using System;

namespace SocialMedia.Application.DTOs.GitHubDtos
{
    public class GitHubCommitDto
    {
        public string Sha { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorAvatarUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CommittedAt { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
    }
}
