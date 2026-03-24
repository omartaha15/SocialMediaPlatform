using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Hubs;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class GroupChatController : Controller
    {
        private readonly IGroupChatService _groupChatService;
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupChatController(
            IGroupChatService groupChatService,
            IMessageService messageService,
            IHubContext<ChatHub> hubContext)
        {
            _groupChatService = groupChatService;
            _messageService = messageService;
            _hubContext = hubContext;
        }

        // ── Groups list ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var groups = await _groupChatService.GetUserGroupsAsync(userId);
            var unreadDMs = await _messageService.GetUnreadCountAsync(userId);

            ViewBag.TotalUnread = unreadDMs;
            return View(groups);
        }

        // ── Create group — GET ────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateGroupDto());
        }

        // ── Create group — POST ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var group = await _groupChatService.CreateGroupAsync(userId, dto);
            return RedirectToAction(nameof(Room), new { groupId = group.Id });
        }

        // ── Group chat room ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Room(Guid groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (!await _groupChatService.IsGroupMemberAsync(groupId, userId))
                return Forbid();

            var group = await _groupChatService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound();

            var history = await _groupChatService.GetGroupHistoryAsync(groupId);
            var members = await _groupChatService.GetMembersAsync(groupId);
            var isAdmin = members.Any(m => m.UserId == userId && m.Role == "Admin");
            var unreadDMs = await _messageService.GetUnreadCountAsync(userId);

            ViewBag.GroupId = groupId;
            ViewBag.CurrentUserId = userId;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Members = members;
            ViewBag.TotalUnread = unreadDMs;

            return View(history);
        }

        // ── Send a group message ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(Guid groupId, string content)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) return Unauthorized();

            var dto = new SendGroupMessageDto { GroupId = groupId, Content = content };
            var message = await _groupChatService.SendGroupMessageAsync(senderId, dto);

            await _hubContext.Clients
                .Group($"group_{groupId}")
                .SendAsync("ReceiveGroupMessage", new
                {
                    groupId = message.GroupId,
                    senderId = message.SenderId,
                    senderName = message.SenderName,
                    content = message.Content,
                    sentAt = message.CreatedAt
                });

            return Json(message);
        }

        // ── Add a member (admin only) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(Guid groupId, string targetUserId)
        {
            var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (requesterId == null) return Unauthorized();

            try
            {
                await _groupChatService.AddMemberAsync(groupId, requesterId, targetUserId);
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // ── Remove a member (admin only) ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(Guid groupId, string targetUserId)
        {
            var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (requesterId == null) return Unauthorized();

            try
            {
                await _groupChatService.RemoveMemberAsync(groupId, requesterId, targetUserId);
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // ── Leave a group ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(Guid groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                await _groupChatService.LeaveGroupAsync(groupId, userId);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
