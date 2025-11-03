using System.ComponentModel.DataAnnotations;

namespace serverless_auth.ViewModels
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Application ID is required.")]
        public string ApplicationID { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsEnabled { get; set; }
    }
}
