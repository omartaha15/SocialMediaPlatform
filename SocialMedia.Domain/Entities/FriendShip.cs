using SocialMedia.Domain.Common;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class FriendShip: BaseEntity
    {
        public FriendShipStatus Status { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser? Receiver { get; set; }

    }
}
