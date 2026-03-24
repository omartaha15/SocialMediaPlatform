using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialMedia.Infrastructure.Repositories
{
    public class FriendshipRepository : Repository<FriendShip>, IFriendshipRepository
    {
        public FriendshipRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<FriendShip?> GetFriendshipAsync(string userIdA, string userIdB)
        {
            return await _context.Set<FriendShip>()
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == userIdA && f.ReceiverId == userIdB) ||
                    (f.SenderId == userIdB && f.ReceiverId == userIdA));
        }

        public async Task<IEnumerable<FriendShip>> GetFriendsForUserAsync(string userId)
        {
            return await _context.Set<FriendShip>()
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == FriendShipStatus.Accepted)
                .ToListAsync();
        }

        public async Task<IEnumerable<FriendShip>> GetPendingRequestsForUserAsync(string userId)
        {
            return await _context.Set<FriendShip>()
                .Include(f => f.Sender)
                .Where(f => f.ReceiverId == userId && f.Status == FriendShipStatus.pending)
                .ToListAsync();
        }

        public async Task<int> GetFriendsCountAsync(string userId)
        {
            return await _context.Set<FriendShip>()
                .CountAsync(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == FriendShipStatus.Accepted);
        }


    }
}
