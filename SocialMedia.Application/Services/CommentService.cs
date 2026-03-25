using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.PostDtos.CommentDtos;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _uow;

        public CommentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<CommentDto>> GetPostCommentsAsync(Guid postId)
        {
            var comments = await _uow.Repository<Comment>()
                .Query()
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.Author)                              // ✅ was Author
                .Include(c => c.Replies).ThenInclude(r => r.Author) // ✅ was Author
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(MapToDto).ToList();
        }

        public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto, string userId)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                PostId = dto.PostId,
                ParentCommentId = dto.ParentCommentId,
                AuthorId = userId                                     // ✅ was AuthorId
            };

            await _uow.Repository<Comment>().AddAsync(comment);
            await _uow.CompleteAsync();

            // Re-query to eager-load User for the returned DTO
            var saved = await _uow.Repository<Comment>()
                .Query()
                .Include(c => c.Author)                              // ✅ was Author
                .FirstAsync(c => c.Id == comment.Id);

            return MapToDto(saved);
        }

        public async Task DeleteCommentAsync(Guid commentId, string requestingUserId)
        {
            var comment = await _uow.Repository<Comment>()
                .Query()
                .FirstOrDefaultAsync(c => c.Id == commentId)
                ?? throw new KeyNotFoundException("Comment not found.");

            if (comment.AuthorId != requestingUserId)                // ✅ was AuthorId
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            _uow.Repository<Comment>().Delete(comment);
            await _uow.CompleteAsync();
        }

        private static CommentDto MapToDto(Comment c) => new()
        {
            Id = c.Id,
            Content = c.Content,
            PostId = c.PostId,
            ParentCommentId = c.ParentCommentId,
            AuthorId = c.AuthorId,                                    
            AuthorName = c.Author?.UserName ?? "Unknown",                    
            AuthorAvatarUrl= c.Author?.ProfilePictureUrl,                    
            CreatedAt = c.CreatedAt,
            Replies = c.Replies?.Select(MapToDto).ToList() ?? new()
        };
    }
}       