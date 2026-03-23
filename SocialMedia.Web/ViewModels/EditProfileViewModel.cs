using SocialMedia.Domain.Enums;

namespace SocialMedia.Web.ViewModels
{
    public class EditProfileViewModel
    {
        public string UserName { get; set; }

        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
