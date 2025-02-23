using iRLeagueApiCore.Server.Handlers.Teams;

namespace iRLeagueApiCore.Server.Validation.Teams;

public class PutTeamRequestValidator : AbstractValidator<PutTeamRequest>
{
    public PutTeamRequestValidator()
    {
        RuleFor(x => x.Model.Members)
            .Empty()
            .When(x => x.Model.IsArchived)
            .WithMessage("Members list must be empty when Team is archived (IsArchived == true)");
    }
}
