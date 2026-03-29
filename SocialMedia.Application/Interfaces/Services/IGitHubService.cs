using SocialMedia.Application.DTOs.GitHubDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IGitHubService
    {
        Task<IEnumerable<GitHubCommitDto>> GetRecentCommitsAsync(string owner, string repo, int limit = 5);
    }
}
