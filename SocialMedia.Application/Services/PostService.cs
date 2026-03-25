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

        public PostService(IUnitOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task CreatePostAsync(CreatePostDto dto)
        {
            // Upload image using service

            var post = new Post
            {
                Content = dto.Content,
                UserId = dto.UserId,
                ImageUrl = dto.ImageUrl, 
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Post>().AddAsync(post);
            await _unitOfWork.CompleteAsync();
        }

      

        public Task DeletePostAsync(Guid id)
        {

            var post =  _unitOfWork.Repository<Post>().GetByIdAsync(id).Result;

            if (post == null)
                throw new Exception("Post not found");

            _unitOfWork.Repository<Post>().Delete(post);

            return _unitOfWork.CompleteAsync();
        }

       
        public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
        {
            var posts = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
.OrderByDescending(p => p.CreatedAt)
    .ToListAsync();
   

            return posts.Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                userId = p.UserId,
                UserName = p.Creator?.UserName ?? "Unknown",
                CreatedAt = p.CreatedAt,
                ImageUrl = p.ImageUrl
            });
        }

        public async Task<PostDto?> GetPostByIdAsync(Guid id)
        {
            var post = await _unitOfWork.Repository<Post>()
                .Query()
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return null;

            return new PostDto
            {
                Id        = post.Id,
                Content   = post.Content,
                userId    = post.UserId,
                UserName  = post.Creator?.UserName ?? "Unknown",
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
                UserName  = p.Creator?.UserName ?? "Unknown"
            });
        }

    
        public  async Task UpdatePostAsync(UpdatePostDto dto)
        {
            var post = _unitOfWork.Repository<Post>().GetByIdAsync(dto.Id).Result;

            if(post == null)
                throw new Exception("Post not found");

            post.Content = dto.Content;
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                post.ImageUrl = dto.ImageUrl;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                post.ImageUrl = dto.ImageUrl;
            _unitOfWork.Repository<Post>().Update(post);

            await _unitOfWork.CompleteAsync();


        }



    }
}
