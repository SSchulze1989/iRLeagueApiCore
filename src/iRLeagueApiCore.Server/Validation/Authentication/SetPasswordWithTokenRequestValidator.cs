using iRLeagueApiCore.Server.Handlers.Authentication;
using System.Text.RegularExpressions;

namespace iRLeagueApiCore.Server.Validation.Authentication;

public class SetPasswordWithTokenRequestValidator : AbstractValidator<SetPasswordWithTokenRequest>
{
    public SetPasswordWithTokenRequestValidator()
    {
        RuleFor(x => x.Model.NewPassword)
            .NotEmpty()
            .WithMessage("Password cannot be empty")
            .Must(PasswordIsValid)
            .WithMessage("Invalid password. Password must have at least 6 characters, must contain one upper- and lowercase letter, one number and one special character ($!%*?&)");
    }

    private bool PasswordIsValid(string password)
    {
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
    }
}
