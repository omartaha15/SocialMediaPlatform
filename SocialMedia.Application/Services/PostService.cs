using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.PostDtos;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IDashboardNotifierService _dashboardNotifier;
        public PostService(IUnitOfWork unitOfWork, IImageService imageService, IDashboardNotifierService dashboardNotifier)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _dashboardNotifier = dashboardNotifier;
        }

        public async Task CreatePostAsync(CreatePostDto dto)
        {
  


            var post = new Post
            {
                Content = dto.Content,
                UserId = dto.UserId,
                ImageUrl = dto.ImageUrl, 
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Post>().AddAsync(post);
            await _unitOfWork.CompleteAsync();
            await _dashboardNotifier.NotifyDashboardUpdatedAsync();
        }

      

        public async Task DeletePostAsync(Guid id)
        {
            var post = await _unitOfWork.Repository<Post>().GetByIdAsync(id);

            if (post == null)
                throw new KeyNotFoundException("Post not found");

            // Delete the post image from disk if it exists
            if (!string.IsNullOrEmpty(post.ImageUrl))
                _imageService.DeleteImageAsync(post.ImageUrl);

            _unitOfWork.Repository<Post>().Delete(post);
            await _unitOfWork.CompleteAsync();
            await _dashboardNotifier.NotifyDashboardUpdatedAsync();
        }

       
        public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
        {
            var posts = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                userId = p.UserId,
                CommentCount = p.Comments.Count,
                UserName = p.Creator?.UserName ?? "Unknown",
                UserAvatarUrl = p.Creator?.ProfilePictureUrl,
                CreatedAt = p.CreatedAt,
                ImageUrl = p.ImageUrl
            });
        }

        public async Task<(IEnumerable<PostDto> Posts, bool HasMore)> GetPagedPostsAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalCount = await _unitOfWork.Repository<Post>().Query().CountAsync();

            var posts = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var dtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                userId = p.UserId,
                CommentCount = p.Comments.Count,
                UserName = p.Creator?.UserName ?? "Unknown",
                UserAvatarUrl = p.Creator?.ProfilePictureUrl,
                CreatedAt = p.CreatedAt,
                ImageUrl = p.ImageUrl
            });

            bool hasMore = skip + pageSize < totalCount;
            return (dtos, hasMore);
        }

        public async Task<PostDto?> GetPostByIdAsync(Guid id)
        {
            var post = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return null;

            return new PostDto
            {
                Id        = post.Id,
                Content   = post.Content,
                userId    = post.UserId,
                CommentCount = post.Comments.Count,
                UserName  = post.Creator?.UserName ?? "Unknown",
                UserAvatarUrl = post.Creator?.ProfilePictureUrl,
                CreatedAt = post.CreatedAt,
                ImageUrl  = post.ImageUrl
            };
        }

        public async Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(Guid userId)
        {
            var userIdStr = userId.ToString();
            var posts = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userIdStr)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => new PostDto
            {
                Id        = p.Id,
                Content   = p.Content,
                ImageUrl  = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                userId    = p.UserId,
                CommentCount = p.Comments.Count,
                UserName  = p.Creator?.UserName ?? "Unknown",
                UserAvatarUrl = p.Creator?.ProfilePictureUrl
            });
        }

    
        public async Task UpdatePostAsync(UpdatePostDto dto)
        {
            var post = await _unitOfWork.Repository<Post>().GetByIdAsync(dto.Id);

            if (post == null)
                throw new KeyNotFoundException("Post not found");

            post.Content = dto.Content;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                post.ImageUrl = dto.ImageUrl;

            _unitOfWork.Repository<Post>().Update(post);
            await _unitOfWork.CompleteAsync();
        }



    }
}
