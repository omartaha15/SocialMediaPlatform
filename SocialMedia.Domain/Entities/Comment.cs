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
        public string Content { get; set; }
        public Guid PostId { get; set; }
        public virtual Post? Post { get; set; }

        public string UserId { get; set; }=string.Empty;
        public virtual ApplicationUser? User { get; set; }
    }
}
