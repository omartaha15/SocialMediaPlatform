using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.ProfileDTOs
{
    public class ProfileDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
    }
}
