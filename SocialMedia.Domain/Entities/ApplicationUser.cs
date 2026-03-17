
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
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? Location { get; set; }


    }
}
