using iRLeagueApiCore.Server.Handlers.Admin;

namespace iRLeagueApiCore.Server.Validation
{
    public class ListUsersRequestValidator : AbstractValidator<ListUsersRequest>
    {
        public ListUsersRequestValidator()
        {
            RuleFor(x => x.LeagueName).NotEmpty()
                .WithMessage("'leagueName' may not be empty");
        }
    }
}
