using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs.PostDtos;
using SocialMedia.Application.Interfaces.Services;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IImageService _imageService;
        public PostController(IPostService postService, IImageService imageService)
        {
            _postService = postService;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();
            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreatePostDto dto, IFormFile image)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            dto.UserId = userIdString;
            var imageUrl = await _imageService.UploadImageAsync(image);

            if (imageUrl != null)
            {
                dto.ImageUrl = imageUrl;
            }

            await _postService.CreatePostAsync(dto);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Ownership check: only post owner can delete
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();
            if (post.userId != userId)
                return Forbid();

            await _postService.DeletePostAsync(postId);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            // Ownership check: only post owner can edit
            if (post.userId != userId)
                return Forbid();

            var dto = new UpdatePostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(UpdatePostDto dto, IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Ownership check
            var existingPost = await _postService.GetPostByIdAsync(dto.Id);
            if (existingPost == null)
                return NotFound();
            if (existingPost.userId != userId)
                return Forbid();

            // Delete old image if a new one is uploaded
            if (image != null && image.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                {
                    _imageService.DeleteImageAsync(existingPost.ImageUrl);
                }
                dto.ImageUrl = await _imageService.UploadImageAsync(image);
            }

            await _postService.UpdatePostAsync(dto);

            return RedirectToAction("Index", "Home");
        }
    }
}