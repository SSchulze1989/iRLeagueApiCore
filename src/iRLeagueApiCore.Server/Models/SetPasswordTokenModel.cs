namespace iRLeagueApiCore.Server.Models
{
    public class SetPasswordTokenModel
    {
        public string PasswordToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
