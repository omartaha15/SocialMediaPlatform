using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs.PostDtos;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Application.Services;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IImageService _imageService;
        public PostController(IPostService postService , IImageService imageService )
        {
            _postService = postService;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();
            return View(posts);
        }
        public async Task<IActionResult> CreatePost(CreatePostDto dto, IFormFile image)
        {

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            dto.UserId = userIdString.ToString();
            var imageUrl = await _imageService.UploadImageAsync(image);

            dto.ImageUrl = imageUrl;

            await _postService.CreatePostAsync(dto);

            return RedirectToAction("Index");
        }
    }
}