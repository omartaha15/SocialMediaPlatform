using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var friends = await _friendshipService.GetFriendsListAsync(userId);
            var pendingRequests = await _friendshipService.GetPendingRequestsListAsync(userId);
            var sentRequests = await _friendshipService.GetSentRequestsListAsync(userId);
            var count = await _friendshipService.GetFriendsCountAsync(userId);

            ViewBag.FriendsCount = count;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.SentRequests = sentRequests;

            return View(friends);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest ( string receiverId )
        {
            var senderId = GetUserId();
            var result = await _friendshipService.SendRequestAsync( senderId, receiverId );

            if ( result )
            {
                TempData [ "SuccessMessage" ] = "Friend request sent!";
            }
            else
            {
                TempData [ "ErrorMessage" ] = "Could not send friend request. You may already be friends or have a pending request.";
            }

            if ( Request.Headers.ContainsKey( "Referer" ) )
            {
                return Redirect( Request.Headers [ "Referer" ].ToString() );
            }
            return RedirectToAction( nameof( Suggestions ) );
        }
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string senderId)
        {
            var receiverId = GetUserId();
            var result = await _friendshipService.AcceptRequestAsync(receiverId, senderId);

            if (result)
            {
                TempData["SuccessMessage"] = "Friend request accepted!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not accept friend request.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = GetUserId();
            var result = await _friendshipService.RemoveFriendAsync(userId, friendId);

            if (result)
            {
                TempData["SuccessMessage"] = "Friend removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove friend.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Suggestions(int pageNumber = 1)
        {
            var userId = GetUserId();
            var suggestions = await _friendshipService.GetFriendSuggestionsAsync(userId, pageNumber);
            return View(suggestions);
        }
    }
}
