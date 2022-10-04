using System.ComponentModel.DataAnnotations;

namespace iRLeagueApiCore.Server.Authentication
{
    /// <summary>
    /// Model containing user data used for creating a new user
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// User name used for login later
        /// </summary>
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Valid email
        /// </summary>
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
