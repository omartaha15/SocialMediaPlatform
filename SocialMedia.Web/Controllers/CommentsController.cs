using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs.PostDtos.CommentDtos;
using SocialMedia.Application.Interfaces.Services;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    // Web/Controllers/CommentsController.cs
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
            => _commentService = commentService;

        [HttpGet]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            var comments = await _commentService.GetPostCommentsAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var comment = await _commentService.AddCommentAsync(dto, userId);
            return Ok(comment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _commentService.DeleteCommentAsync(id, userId);
            return Ok();
        }
    }
}
