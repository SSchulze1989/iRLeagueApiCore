using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Admin;

namespace iRLeagueApiCore.Server.Validation
{
    public class TestValidator : AbstractValidator<GiveRoleRequest>
    {
        public TestValidator()
        {
            RuleFor(x => x.UserName)
                .Must(x => x == "Johnny")
                .WithMessage("Username must be 'Johnny'");
        }
    }
}
