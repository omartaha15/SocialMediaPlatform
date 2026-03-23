
using Microsoft.AspNetCore.Identity;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public DateTime? LastLogin { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; } = "/images/user.jpg";
        public string ? CoverPhotoUrl { get; set; } = "/images/coverImage.png";
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? Location { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public virtual ICollection<GroupMessages> GroupMessages { get; set; } = new List<GroupMessages>();
        public virtual ICollection<FriendShip> SentFriendRequests { get; set; } = new List<FriendShip>();
        public virtual ICollection<FriendShip> ReceivedFriendRequests { get; set; } = new List<FriendShip>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
        public virtual ICollection<Message> MessagesSent { get; set; } = new List<Message>();
        public virtual ICollection<Message> MessagesReceived { get; set; } = new List<Message>();





    }
}
