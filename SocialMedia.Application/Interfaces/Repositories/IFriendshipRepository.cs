using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Repositories
{
    public interface IFriendshipRepository : IGenericRepository<FriendShip>
    {
        Task<FriendShip?> GetFriendshipAsync(string userIdA, string userIdB);
        Task<IEnumerable<FriendShip>> GetFriendsForUserAsync(string userId);
        Task<IEnumerable<FriendShip>> GetPendingRequestsForUserAsync(string userId);
        Task<IEnumerable<FriendShip>> GetSentRequestsForUserAsync(string userId);
        Task<int> GetFriendsCountAsync(string userId);
    }
}
