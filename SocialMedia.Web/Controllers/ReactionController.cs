using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Enums;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class ReactionController : Controller
    {
        private readonly IReactionService _reactionService;

        public ReactionController(IReactionService reactionService)
        {
            _reactionService = reactionService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> React(Guid postId, MultiReaction type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _reactionService.AddOrUpdateReactionAsync(postId, userId, type);

            var (counts, userReaction) = await _reactionService.GetReactionSummaryAsync(postId, userId);

            return Json(new
            {
                success = true,
                total = counts.Values.Sum(),
                userReaction = userReaction?.ToString(),
                counts = counts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _reactionService.RemoveReactionAsync(postId, userId);

            var (counts, _) = await _reactionService.GetReactionSummaryAsync(postId, userId);

            return Json(new
            {
                success = true,
                total = counts.Values.Sum(),
                userReaction = (string?)null,
                counts = counts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value)
            });
        }

        /// <summary>
        /// Returns list of users who reacted to a post (for the reactors modal).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReactors(Guid postId)
        {
            var reactors = await _reactionService.GetReactorsAsync(postId);
            return Json(reactors.Select(r => new
            {
                userId = r.UserId,
                userName = r.UserName,
                avatarUrl = r.AvatarUrl ?? "/images/user.jpg",
                reactionType = r.ReactionType.ToString()
            }));
        }
    }
}
