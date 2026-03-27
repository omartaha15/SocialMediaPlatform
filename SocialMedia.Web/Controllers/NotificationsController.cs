using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> List(int take = 20, int skip = 0)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, take, skip);
            return Ok(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var updated = await _notificationService.MarkAsReadAsync(userId, id);
            if (!updated) return NotFound();

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var updatedCount = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true, updatedCount });
        }
    }
}
