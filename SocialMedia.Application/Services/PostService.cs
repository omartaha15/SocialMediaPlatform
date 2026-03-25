using Microsoft.AspNetCore.Http;
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

      

        public Task DeletePostAsync(int id)
        {
            throw new NotImplementedException();
        }

        //public async Task DeletePostAsync(Guid id, Guid userId)
        //{
        //    var post = await _unitOfWork.Repository<Post>().GetByIdAsync(id);

        //    if (post == null)
        //        throw new Exception("Post not found");

        //    if (post.UserId != userId)
        //        throw new UnauthorizedAccessException();

        //    _unitOfWork.Repository<Post>().Delete(post);
        //    await _unitOfWork.CompleteAsync();
        //}

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
        {
            var posts = await _unitOfWork.Repository<Post>().GetAllAsync();

            var result = posts.Select(p => new PostDto
            {
                Id = p.Id ,
                Content = p.Content,
                userId = p.UserId,
                 UserName = "Abdo",
                CreatedAt = p.CreatedAt,
                ImageUrl = p.ImageUrl
            });

            return result;
        }
    }
}
