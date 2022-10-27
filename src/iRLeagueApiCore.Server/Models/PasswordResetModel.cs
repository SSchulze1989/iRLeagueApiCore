namespace iRLeagueApiCore.Server.Models
{
    public class PasswordResetModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
