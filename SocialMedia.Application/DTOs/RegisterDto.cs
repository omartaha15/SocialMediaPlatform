using Microsoft.AspNetCore.Http;
using SocialMedia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs
{
    public class RegisterDto
    {
        public string UserName { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; } 
        public string ConfirmPassword { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get;set; }
        public IFormFile? ProfileImage { get; set; }

    }
}
