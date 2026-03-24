using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.DTOs.ChatDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Hubs;
using SocialMedia.Web.ViewModels;
using SocialMedia.Web.ViewModels.Chat;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class MessagingController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagingController(
            IMessageService messageService,
            IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        // ── Conversations list ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var conversations = await _messageService.GetConversationsAsync(userId);
            var unreadCount = await _messageService.GetUnreadCountAsync(userId);

            var vm = new ConversationsViewModel
            {
                Conversations = conversations.ToList(),
                TotalUnread = unreadCount
            };

            return View(vm);
        }

        // ── Open chat window with a specific user ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Chat(string otherUserId, string otherUserName = "", string? otherUserProfilePicture = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var messages = await _messageService.GetChatHistoryAsync(userId, otherUserId);
            await _messageService.MarkAsReadAsync(userId, otherUserId);

            var vm = new ChatViewModel
            {
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                OtherUserProfilePicture = otherUserProfilePicture,
                History = messages.ToList()
            };

            return View(vm);
        }

        // ── Send a direct message ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) return Unauthorized();

            var dto = new SendMessageDto { ReceiverId = receiverId, Content = content };
            var message = await _messageService.SendMessageAsync(senderId, dto);

            await _hubContext.Clients
                .Group(receiverId)
                .SendAsync("ReceiveDirectMessage", new
                {
                    senderId = message.SenderId,
                    senderName = message.SenderName,
                    content = message.Content,
                    sentAt = message.CreatedAt
                });

            return Json(message);
        }

        // ── Mark messages as read ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _messageService.MarkAsReadAsync(userId, otherUserId);
            return Ok();
        }

        // ── Unread count for navbar badge ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var count = await _messageService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }
    }
}
