using SocialMedia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Post:BaseEntity
    {
        public string Content { get; set; }
        public string? ImageUrl { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? Creator { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
         public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
