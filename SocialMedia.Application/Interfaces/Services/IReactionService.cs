using SocialMedia.Application.DTOs.PostDtos.ReactionDtos;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IReactionService
    {
        Task AddOrUpdateReactionAsync(Guid postId, string userId, MultiReaction reaction);
        Task RemoveReactionAsync(Guid postId, string userId);
        Task<Dictionary<MultiReaction, int>> GetReactionsCountAsync(Guid postId);

        /// <summary>
        /// Returns per-reaction counts and the calling user's current reaction.
        /// </summary>
        Task<(Dictionary<MultiReaction, int> Counts, MultiReaction? UserReaction)>
            GetReactionSummaryAsync(Guid postId, string userId);

        /// <summary>
        /// Returns all users who reacted to a post, with their reaction type.
        /// </summary>
        Task<List<ReactorDto>> GetReactorsAsync(Guid postId);
    }
}

