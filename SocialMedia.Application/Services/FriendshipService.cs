using Microsoft.AspNetCore.Identity;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendshipService(IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _friendshipRepository = friendshipRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<bool> SendRequestAsync(string senderId, string receiverId)
        {
            // Prevent self-friending
            if (senderId == receiverId) return false;

            // Prevent duplicate requests
            var existing = await _friendshipRepository.GetFriendshipAsync(senderId, receiverId);
            if (existing != null) return false;

            var friendship = new FriendShip
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendShipStatus.pending
            };

            await _friendshipRepository.AddAsync(friendship);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> AcceptRequestAsync(string receiverId, string senderId)
        {
            var existing = await _friendshipRepository.GetFriendshipAsync(senderId, receiverId);
            if (existing == null || existing.Status != FriendShipStatus.pending) return false;

            // Ensure only the receiver can accept
            if (existing.ReceiverId != receiverId) return false;

            existing.Status = FriendShipStatus.Accepted;
            existing.UpdatedAt = DateTime.UtcNow;

            _friendshipRepository.Update(existing);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> RemoveFriendAsync(string userIdA, string userIdB)
        {
            var existing = await _friendshipRepository.GetFriendshipAsync(userIdA, userIdB);
            if (existing == null) return false;

            _friendshipRepository.Delete(existing);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<IEnumerable<ApplicationUser>> GetFriendsListAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetFriendsForUserAsync(userId);
            
            var friends = friendships.Select(f => f.SenderId == userId ? f.Receiver : f.Sender)
                .Where(u => u != null)
                .ToList();

            return friends!;
        }

        public async Task<IEnumerable<ApplicationUser>> GetPendingRequestsListAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetPendingRequestsForUserAsync(userId);

            var senders = friendships.Select(f => f.Sender)
                .Where(u => u != null)
                .ToList();

            return senders!;
        }

        public async Task<IEnumerable<ApplicationUser>> GetFriendSuggestionsAsync(string userId, int limit = 10)
        {
            // Get all friendships (both accepted and pending)
            var allFriendships = await _friendshipRepository.GetAllAsync();
            var userFriendships = allFriendships.Where(f => f.SenderId == userId || f.ReceiverId == userId);
            
            // Extract the IDs of the people involved
            var friendIds = userFriendships
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToHashSet();
            
            // Query for users that aren't the current user and aren't in the friendIds hashset
            var suggestions = await _userManager.Users
                .Where(u => u.Id != userId && !friendIds.Contains(u.Id))
                .Take(limit)
                .ToListAsync();

            return suggestions;
        }

        public async Task<int> GetFriendsCountAsync(string userId)
        {
            return await _friendshipRepository.GetFriendsCountAsync(userId);
        }
    }
}
