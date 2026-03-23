using SocialMedia.Domain.Enums;

namespace SocialMedia.Web.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }

        public string? Bio { get; set; }
        public string? Location { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
    }
}
