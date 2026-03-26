using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.Search;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Repositories
{
    public class UserSearchRepository : IUserSearchRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSearchRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(List<UserSearchDto> Data, int TotalCount)> SearchUsersAsync(
            string trimmedQuery,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var likeLiteral = EscapeLikePattern(trimmedQuery);
            var startsWithPattern = likeLiteral + "%";
            var containsPattern = "%" + likeLiteral + "%";

            var usersQuery = _userManager.Users;

            var startsWithQuery = usersQuery
                .Where(u =>
                    (u.UserName != null && EF.Functions.Like(u.UserName, startsWithPattern)) ||
                    (u.Email != null && EF.Functions.Like(u.Email, startsWithPattern)));

            var containsQuery = usersQuery
                .Where(u =>
                    (u.UserName != null && EF.Functions.Like(u.UserName, containsPattern)) ||
                    (u.Email != null && EF.Functions.Like(u.Email, containsPattern)));

            var combinedQuery = startsWithQuery.Union(containsQuery);

            var totalCount = await combinedQuery.CountAsync(cancellationToken);

            var users = await combinedQuery
                .OrderBy(u => u.UserName)
                .ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSearchDto
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    ProfileImage = u.ProfilePictureUrl
                })
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}
