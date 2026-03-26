using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Web.ViewModels.GroupChat
{
    public class CreateGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
