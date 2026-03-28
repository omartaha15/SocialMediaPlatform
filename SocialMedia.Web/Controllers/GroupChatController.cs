using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Hubs;
using SocialMedia.Web.ViewModels;
using SocialMedia.Web.ViewModels.GroupChat;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class GroupChatController : Controller
    {
        private readonly IGroupChatService _groupChatService;
        private readonly IMessageService _messageService;
        private readonly IFriendshipService _friendshipService;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupChatController(
            IGroupChatService groupChatService,
            IMessageService messageService,
            IFriendshipService friendshipService,
            IHubContext<ChatHub> hubContext)
        {
            _groupChatService = groupChatService;
            _messageService = messageService;
            _friendshipService = friendshipService;
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
        public IActionResult Create() => View(new CreateGroupViewModel());

        // ── Create group — POST ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var dto = new CreateGroupDto { Name = vm.Name, Description = vm.Description };
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
            var unreadDMs = await _messageService.GetUnreadCountAsync(userId);

            var memberIds = members.Select(m => m.UserId).ToHashSet();
            var availableFriendsToAdd = (await _friendshipService.GetFriendsListAsync(userId))
                .Where(f => !memberIds.Contains(f.Id))
                .OrderBy(f => f.UserName)
                .Select(f => new SocialMedia.Application.DTOs.ProfileDTOs.UserSuggestionDto
                {
                    Id                = f.Id,
                    UserName          = f.UserName ?? string.Empty,
                    ProfilePictureUrl = f.ProfilePictureUrl
                })
                .ToList();

            var vm = new GroupRoomViewModel
            {
                Group = group,
                History = history.ToList(),
                Members = members.ToList(),
                AvailableFriendsToAdd = availableFriendsToAdd,
                IsAdmin = members.Any(m => m.UserId == userId && m.Role == "Admin"),
                CurrentUserId = userId
            };

            ViewBag.TotalUnread = unreadDMs;
            return View(vm);
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
                    messageId = message.Id,
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

        // ── Edit a group message ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(Guid messageId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                var message = await _groupChatService.EditGroupMessageAsync(messageId, userId, content);

                // Broadcast edit to all group members via SignalR
                await _hubContext.Clients
                    .Group($"group_{message.GroupId}")
                    .SendAsync("MessageEdited", new
                    {
                        messageId = message.Id,
                        content = message.Content,
                        isEdited = message.IsEdited,
                        editedAt = message.EditedAt
                    });

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

        // ── Delete a group message ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(Guid messageId, Guid groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                await _groupChatService.DeleteGroupMessageAsync(messageId, userId);

                // Broadcast delete to all group members via SignalR
                await _hubContext.Clients
                    .Group($"group_{groupId}")
                    .SendAsync("MessageDeleted", new { messageId });

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
