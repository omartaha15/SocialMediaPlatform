using SocialMedia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Group: BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public virtual ICollection<GroupMessages> GroupMessages { get; set; } = new List<GroupMessages>();
        public virtual ICollection<GroupMember> GroupMembers { get; set; }= new List<GroupMember>();
    }
}
