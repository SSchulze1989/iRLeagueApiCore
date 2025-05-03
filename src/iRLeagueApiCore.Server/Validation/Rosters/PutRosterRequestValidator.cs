using iRLeagueApiCore.Server.Handlers.Rosters;

namespace iRLeagueApiCore.Server.Validation.Rosters;

public sealed class PutRosterRequestValidator : AbstractValidator<PutRosterRequest>
{
    public PutRosterRequestValidator(PostRosterModelValidator modelValidator)
    {
        RuleFor(x => x.Model)
            .SetValidator(modelValidator);
    }
}
