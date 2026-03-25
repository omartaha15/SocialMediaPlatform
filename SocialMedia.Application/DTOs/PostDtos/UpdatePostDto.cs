using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.PostDtos
{
    public class UpdatePostDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
    }
}
