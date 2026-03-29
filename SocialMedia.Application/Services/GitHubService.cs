using Newtonsoft.Json;
using SocialMedia.Application.DTOs.GitHubDtos;
using SocialMedia.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // GitHub API requires a User-Agent header
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "SocialMediaApp-Client");
            }
        }

        public async Task<IEnumerable<GitHubCommitDto>> GetRecentCommitsAsync(string owner, string repo, int limit = 5)
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/commits?per_page={limit}";
            
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return Enumerable.Empty<GitHubCommitDto>();

                var content = await response.Content.ReadAsStringAsync();
                var commits = JsonConvert.DeserializeObject<List<GitHubCommitResponse>>(content);

                return commits?.Select(c => new GitHubCommitDto
                {
                    Sha = c.sha,
                    AuthorName = c.commit.author.name,
                    AuthorAvatarUrl = c.author?.avatar_url ?? "",
                    Message = c.commit.message,
                    CommittedAt = c.commit.author.date,
                    HtmlUrl = c.html_url,
                    RepositoryName = repo
                }) ?? Enumerable.Empty<GitHubCommitDto>();
            }
            catch
            {
                return Enumerable.Empty<GitHubCommitDto>();
            }
        }

        // Inner classes for GitHub API JSON mapping
        private class GitHubCommitResponse
        {
            public string sha { get; set; } = string.Empty;
            public CommitData commit { get; set; } = new();
            public AuthorData? author { get; set; }
            public string html_url { get; set; } = string.Empty;
        }

        private class CommitData
        {
            public string message { get; set; } = string.Empty;
            public AuthorInfo author { get; set; } = new();
        }

        private class AuthorInfo
        {
            public string name { get; set; } = string.Empty;
            public DateTime date { get; set; }
        }

        private class AuthorData
        {
            public string avatar_url { get; set; } = string.Empty;
        }
    }
}
