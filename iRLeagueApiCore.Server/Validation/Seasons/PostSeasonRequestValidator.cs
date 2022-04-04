using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Seasons;

namespace iRLeagueApiCore.Server.Validation.Seasons
{
    public class PostSeasonRequestValidator : AbstractValidator<PostSeasonRequest>
    {
        public PostSeasonRequestValidator(PostSeasonModelValidator modelValidator)
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("League id required");
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
