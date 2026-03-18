using SocialMedia.Domain.Common;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class GroupMember: BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        public Guid GroupId { get; set; }
        public virtual Group? Group { get; set; }
        public GroupRole Role { get; set; }
    }
}
