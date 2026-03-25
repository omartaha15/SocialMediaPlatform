using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.PostDtos.CommentDtos
{
    public class CreateCommentDto
    {
        [Required, MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Guid? ParentCommentId { get; set; }
    }
}
