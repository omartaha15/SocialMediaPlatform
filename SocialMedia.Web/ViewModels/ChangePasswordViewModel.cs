using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Web.ViewModels
{
    public class ChangePasswordViewModel
    {

        [Required(ErrorMessage = "Current Password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter and one number.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm New Password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}