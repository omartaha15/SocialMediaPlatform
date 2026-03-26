using SocialMedia.Application.DTOs.Search;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface ISearchService
    {
        Task<SearchResultDto<UserSearchDto>> SearchUsersAsync(SearchRequestDto request);
    }
}