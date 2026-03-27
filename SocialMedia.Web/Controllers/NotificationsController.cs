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
        public async Task<IActionResult> List(int take = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, take);
            return Ok(notifications);
        }
    }
}
