using System.ComponentModel.DataAnnotations;

namespace iRLeagueApiCore.Server.Authentication
{
    /// <summary>
    /// Model containing user data used for login
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// User name as registered
        /// </summary>
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }
        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
