using SocialMedia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Comment: BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
        public string AuthorId { get; set; } = string.Empty;
        public ApplicationUser Author { get; set; } = null!;

        
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
