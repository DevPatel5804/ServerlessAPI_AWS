using System.ComponentModel.DataAnnotations;

namespace serverless_auth.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Application ID is required.")]
        public string ApplicationID { get; set; } // Added this field

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
