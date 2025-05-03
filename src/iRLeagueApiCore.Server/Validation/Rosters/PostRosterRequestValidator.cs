using iRLeagueApiCore.Server.Handlers.Rosters;

namespace iRLeagueApiCore.Server.Validation.Rosters;

public sealed class PostRosterRequestValidator : AbstractValidator<PostRosterRequest>
{
    public PostRosterRequestValidator(PostRosterModelValidator modelValidator)
    {
        RuleFor(x => x.Model)
            .SetValidator(modelValidator);
    }
}
