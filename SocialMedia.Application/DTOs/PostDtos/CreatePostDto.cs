using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.PostDtos
{

    public class CreatePostDto
    {
        public string UserId { get; set; }

        public string? ImageUrl { get; set; }
        public string Content { get; set; }

    }

}
