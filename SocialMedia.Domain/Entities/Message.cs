using SocialMedia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Message: BaseEntity
    {
        public string Content { get; set; }
        public bool IsRead { get; set; } = false;
        public string SenderId { get; set; } = string.Empty;
        public virtual ApplicationUser? Sender { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public virtual ApplicationUser? Receiver { get; set; }
    }
}
