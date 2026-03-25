using SocialMedia.Application.DTOs.PostDtos.CommentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetPostCommentsAsync(Guid postId);
        Task<CommentDto> AddCommentAsync(CreateCommentDto dto, string authorId);
        Task DeleteCommentAsync(Guid commentId, string requestingUserId);
    }
}
