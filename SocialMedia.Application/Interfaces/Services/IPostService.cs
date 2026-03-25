using Microsoft.AspNetCore.Http;
using SocialMedia.Application.DTOs.PostDtos;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IPostService
    {
        Task CreatePostAsync(CreatePostDto dto );
       Task<IEnumerable<PostDto>> GetAllPostsAsync();

       // Task<PostDto?> GetPostByIdAsync(int id);

      //  Task UpdatePostAsync(UpdatePostDto dto);

        Task DeletePostAsync(int id);
    }


}
