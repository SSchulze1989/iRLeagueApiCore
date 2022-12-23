using iRLeagueApiCore.Server.Handlers.Leagues;

namespace iRLeagueApiCore.Server.Validation.Leagues
{
    public class PostLeagueRequestValidator : AbstractValidator<PostLeagueRequest>
    {
        public PostLeagueRequestValidator(PostLeagueModelValidator modelValidator)
        {
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
