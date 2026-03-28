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

            if (string.IsNullOrWhiteSpace(otherUserName) || string.IsNullOrWhiteSpace(otherUserProfilePicture))
            {
                var conversation = (await _messageService.GetConversationsAsync(userId))
                    .FirstOrDefault(c => c.OtherUserId == otherUserId);

                if (string.IsNullOrWhiteSpace(otherUserName))
                    otherUserName = conversation?.OtherUserName ?? string.Empty;

                if (string.IsNullOrWhiteSpace(otherUserProfilePicture))
                    otherUserProfilePicture = conversation?.OtherUserProfilePicture;

                var otherUserLastMessage = messages
                    .Where(m => m.SenderId == otherUserId)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(otherUserName))
                    otherUserName = otherUserLastMessage?.SenderName ?? "User";

                if (string.IsNullOrWhiteSpace(otherUserProfilePicture))
                    otherUserProfilePicture = otherUserLastMessage?.SenderProfilePicture;
            }

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

            var payload = new
            {
                messageId = message.Id,
                senderId = message.SenderId,
                senderName = message.SenderName,
                content = message.Content,
                sentAt = message.CreatedAt
            };

            await Task.WhenAll(
                _hubContext.Clients.Group(receiverId).SendAsync("ReceiveDirectMessage", payload),
                _hubContext.Clients.Group(senderId).SendAsync("ReceiveDirectMessage", payload));

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

        // ── Online status check (used by Chat view header) ────────────────────
        [HttpGet]
        public IActionResult IsOnline(string userId)
        {
            var online = ChatHub.IsUserOnline(userId);
            return Json(new { online });
        }

        // ── Edit a direct message ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(Guid messageId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                var message = await _messageService.EditMessageAsync(messageId, userId, content);

                // Broadcast edit to both users via SignalR
                var payload = new
                {
                    messageId = message.Id,
                    content = message.Content,
                    isEdited = message.IsEdited,
                    editedAt = message.EditedAt
                };

                await Task.WhenAll(
                    _hubContext.Clients.Group(message.ReceiverId).SendAsync("MessageEdited", payload),
                    _hubContext.Clients.Group(message.SenderId).SendAsync("MessageEdited", payload));

                return Json(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ── Delete a direct message ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                // Get message details before deleting (to know who to notify)
                var messages = await _messageService.GetChatHistoryAsync(userId, userId, 1, 1000);
                var message = messages.FirstOrDefault(m => m.Id == messageId);
                
                if (message == null)
                    return NotFound(new { error = "Message not found." });

                await _messageService.DeleteMessageAsync(messageId, userId);

                // Broadcast delete to both users via SignalR
                var payload = new { messageId };

                await Task.WhenAll(
                    _hubContext.Clients.Group(message.ReceiverId).SendAsync("MessageDeleted", payload),
                    _hubContext.Clients.Group(message.SenderId).SendAsync("MessageDeleted", payload));

                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
