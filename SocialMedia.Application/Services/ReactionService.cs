using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class ReactionService : IReactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddOrUpdateReactionAsync(Guid postId, string userId, MultiReaction reaction)
        {
            var post = await _unitOfWork.Repository<Post>().GetByIdAsync(postId);

            if (post == null)
                throw new Exception("Post not found");

            var repo = _unitOfWork.Repository<Reaction>();

            var existing = (await repo.FindAsync(r =>
                r.PostId == postId && r.UserId == userId))
                .FirstOrDefault();

            if (existing != null)
            {
                existing.MultiReaction = reaction;
                repo.Update(existing);
            }
            else
            {
                var newReaction = new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    MultiReaction = reaction
                };

                await repo.AddAsync(newReaction);

                if (post.UserId != userId)
                {
                    var sender = await _unitOfWork.FindUserByIdAsync(userId);
                    var senderName = sender?.UserName ?? "Someone";

                    await _unitOfWork.Repository<Notification>().AddAsync(new Notification
                    {
                        UserId = post.UserId,
                        SenderId = userId,
                        Type = NotificationType.PostReaction,
                        Content = $"{senderName} reacted to your post."
                    });
                }
            }

            await _unitOfWork.CompleteAsync();
        }
        public async Task RemoveReactionAsync(Guid postId, string userId)
        {
            var repo = _unitOfWork.Repository<Reaction>();

            var reaction = (await repo.GetAllAsync(r =>
                r.PostId == postId && r.UserId == userId))
                .FirstOrDefault();

            if (reaction != null)
            {
                repo.Delete(reaction);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<Dictionary<MultiReaction, int>> GetReactionsCountAsync(Guid postId)
        {
            var reactions = await _unitOfWork.Repository<Reaction>()
                .GetAllAsync(r => r.PostId == postId);

            return reactions
                .GroupBy(r => r.MultiReaction)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<(Dictionary<MultiReaction, int> Counts, MultiReaction? UserReaction)>
            GetReactionSummaryAsync(Guid postId, string userId)
        {
            var all = await _unitOfWork.Repository<Reaction>()
                .GetAllAsync(r => r.PostId == postId);

            var counts = all
                .GroupBy(r => r.MultiReaction)
                .ToDictionary(g => g.Key, g => g.Count());

            var userReaction = all
                .FirstOrDefault(r => r.UserId == userId)?.MultiReaction;

            return (counts, userReaction);
        }
    }
}
