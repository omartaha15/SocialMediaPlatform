using SocialMedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SocialMedia.Application.DTOs.Search;
using SocialMedia.Application.Interfaces.Services;

namespace SocialMedia.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public SearchService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<SearchResultDto<UserSearchDto>> SearchUsersAsync(SearchRequestDto request)
        {
            var query = request.Query.ToLower();

            var usersQuery = _userManager.Users;

            // StartsWith condition
            var startsWithQuery = usersQuery
                .Where(u =>
                    (u.UserName != null && u.UserName.ToLower().StartsWith(query)) ||
                    (u.Email != null && u.Email.ToLower().StartsWith(query)));

            // Contains condition
            var containsQuery = usersQuery
                .Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(query)) ||
                    (u.Email != null && u.Email.ToLower().Contains(query)));

            var combinedQuery = startsWithQuery
                .Union(containsQuery);

            var totalCount = await combinedQuery.CountAsync();

            var users = await combinedQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserSearchDto
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    ProfileImage = u.ProfilePictureUrl
                })
                .ToListAsync();

            return new SearchResultDto<UserSearchDto>
            {
                Data = users,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}