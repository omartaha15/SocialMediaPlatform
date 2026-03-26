using SocialMedia.Application.DTOs.Search;

namespace SocialMedia.Application.Interfaces.Repositories
{
    public interface IUserSearchRepository
    {
        Task<(List<UserSearchDto> Data, int TotalCount)> SearchUsersAsync(
            string trimmedQuery,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
