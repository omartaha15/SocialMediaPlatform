using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress (ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
