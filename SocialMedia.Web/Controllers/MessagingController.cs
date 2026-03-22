using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Interfaces;
using SocialMedia.Web.Hubs;
using System.Threading.Tasks;

namespace SocialMedia.Web.Controllers
{
    public class MessagingController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagingController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the conversations list view</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
                return Unauthorized();

            var conversations = await _messageService.GetConversationsAsync(userId);
            return View(conversations);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherUserId"></param>
        /// <returns>Opens chat window with a specific user, loads history</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Chat(string otherUserId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
                return Unauthorized();
            var messages = await _messageService.GetChatHistoryAsync(userId, otherUserId);
            ViewBag.OtherUserId = otherUserId; // Pass to view for JS to use when sending messages
            return View(messages);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var senderId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (senderId == null)
                return Unauthorized();
            var dto = new Application.DTOs.ChatDTOs.SendMessageDto
            {
                ReceiverId = receiverId,
                Content = content
            };
            var message = await _messageService.SendMessageAsync(senderId, dto);
            return Json(message); // Return the sent message as JSON for real-time update
        }
    }
}
