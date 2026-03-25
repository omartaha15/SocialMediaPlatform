using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.PostDtos.ReactionDtos
{
    public class ReactionDto
    {
        public Guid PostId { get; set; }
        public int ReactionCount { get; set; }
        public MultiReaction? CurrentUserReaction { get; set; }
    }
}
