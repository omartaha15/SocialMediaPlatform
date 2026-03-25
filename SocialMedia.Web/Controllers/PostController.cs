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

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostDto dto, IFormFile image)
        {

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            dto.UserId = userIdString.ToString();
            var imageUrl = await _imageService.UploadImageAsync(image);

            dto.ImageUrl = imageUrl;

            await _postService.CreatePostAsync(dto);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            await _postService.DeletePostAsync(postId);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
                return NotFound();

            var dto = new UpdatePostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(UpdatePostDto dto, IFormFile image)
        {
            if (image != null)
            {
                dto.ImageUrl = await _imageService.UploadImageAsync(image);
            }

            await _postService.UpdatePostAsync(dto);

            return RedirectToAction("Index");
        }



    }





}