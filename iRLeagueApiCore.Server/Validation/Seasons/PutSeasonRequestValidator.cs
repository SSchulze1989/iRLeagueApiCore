using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Seasons;

namespace iRLeagueApiCore.Server.Validation.Seasons
{
    public class PutSeasonRequestValidator : AbstractValidator<PutSeasonRequest>
    {
        public PutSeasonRequestValidator(PutSeasonModelValidator modelValidator)
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("League id required");
            RuleFor(x => x.SeasonId)
                .NotEmpty()
                .WithMessage("Season id required");
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
