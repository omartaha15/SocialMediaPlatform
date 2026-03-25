using SocialMedia.Application.DTOs.Search;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Application.Interfaces.Services;

namespace SocialMedia.Application.Services
{
    public class SearchService : ISearchService
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        private readonly IUserSearchRepository _userSearchRepository;

        public SearchService(IUserSearchRepository userSearchRepository)
        {
            _userSearchRepository = userSearchRepository;
        }

        public async Task<SearchResultDto<UserSearchDto>> SearchUsersAsync(SearchRequestDto request)
        {
            var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

            var query = request.Query?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SearchResultDto<UserSearchDto>
                {
                    Data = new List<UserSearchDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var (data, totalCount) = await _userSearchRepository.SearchUsersAsync(query, page, pageSize);

            return new SearchResultDto<UserSearchDto>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
        {
            var p = page < 1 ? 1 : page;
            var s = pageSize < 1 ? DefaultPageSize : pageSize;
            if (s > MaxPageSize)
                s = MaxPageSize;
            return (p, s);
        }
    }
}
