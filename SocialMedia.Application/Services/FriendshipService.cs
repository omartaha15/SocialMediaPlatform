using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Application.DTOs.Pagination;
using SocialMedia.Application.DTOs.ProfileDTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Application.Interfaces.Repositories;

namespace SocialMedia.Application.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationRealtimeService _notificationRealtimeService;
        private readonly ILogger<FriendshipService> _logger;

        public FriendshipService(
            IFriendshipRepository friendshipRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationRealtimeService notificationRealtimeService,
            ILogger<FriendshipService> logger)
        {
            _friendshipRepository = friendshipRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationRealtimeService = notificationRealtimeService;
            _logger = logger;
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

            var sender = await _unitOfWork.FindUserByIdAsync(senderId);
            var senderName = sender?.UserName ?? "Someone";
            var notificationMessage = $"{senderName} sent you a follow request.";

            var notification = new Notification
            {
                UserId = receiverId,
                SenderId = senderId,
                Type = NotificationType.FriendRequest,
                Content = notificationMessage
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);

            var saved = await _unitOfWork.CompleteAsync() > 0;
            if (saved)
            {
                try
                {
                    await _notificationRealtimeService.PushAsync(
                        receiverId,
                        NotificationType.FriendRequest,
                        notificationMessage,
                        notificationId: notification.Id,
                        createdAt: notification.CreatedAt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to push realtime follow notification. TargetUserId: {TargetUserId}",
                        receiverId);
                }
            }

            return saved;
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

        public async Task<IEnumerable<ApplicationUser>> GetSentRequestsListAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetSentRequestsForUserAsync(userId);

            var receivers = friendships.Select(f => f.Receiver)
                .Where(u => u != null)
                .ToList();

            return receivers!;
        }

        public async Task<PaginatedList<UserSuggestionDto>> GetFriendSuggestionsAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            // Get current friends (Accepted)
            var currentFriends = await _friendshipRepository.GetFriendsForUserAsync(userId);
            var currentFriendsIds = currentFriends
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();

            // Get pending requests
            var pendingRequests = await _friendshipRepository.GetPendingRequestsForUserAsync(userId);
            var pendingIds = pendingRequests
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();

            // Build the excluded list
            var excludedIds = new HashSet<string>(currentFriendsIds.Concat(pendingIds));
            excludedIds.Add(userId); // exclude self
            
            // Query for suggestions (mutual friends prioritization handled in DB)
            var query = _userManager.Users
                .Where(u => !excludedIds.Contains(u.Id) && 
                            !u.ReceivedFriendRequests.Any(f => f.SenderId == userId) && 
                            !u.SentFriendRequests.Any(f => f.ReceiverId == userId))
                .Select(u => new UserSuggestionDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    Location = u.Location,
                    MutualFriendsCount = u.SentFriendRequests
                        .Count(f => f.Status == FriendShipStatus.Accepted && currentFriendsIds.Contains(f.ReceiverId))
                        + u.ReceivedFriendRequests
                        .Count(f => f.Status == FriendShipStatus.Accepted && currentFriendsIds.Contains(f.SenderId))
                })
                .OrderByDescending(u => u.MutualFriendsCount)
                .ThenBy(u => u.Id);

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<UserSuggestionDto>(items, count, pageNumber, pageSize);
        }

        public async Task<int> GetFriendsCountAsync(string userId)
        {
            return await _friendshipRepository.GetFriendsCountAsync(userId);
        }
    }
}
