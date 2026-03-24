using SocialMedia.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IFriendshipService
    {
        Task<bool> SendRequestAsync(string senderId, string receiverId);
        Task<bool> AcceptRequestAsync(string receiverId, string senderId);
        Task<bool> RemoveFriendAsync(string userIdA, string userIdB);
        Task<IEnumerable<ApplicationUser>> GetFriendsListAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetPendingRequestsListAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetFriendSuggestionsAsync(string userId, int limit = 10);
        Task<int> GetFriendsCountAsync(string userId);
    }
}
