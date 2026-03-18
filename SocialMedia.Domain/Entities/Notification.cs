using SocialMedia.Domain.Common;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Notification: BaseEntity
    {
        public NotificationType Type { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; } = false;
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        public string? SenderId { get; set; } = string.Empty;
        public virtual ApplicationUser? Sender { get; set; }

    }
}
