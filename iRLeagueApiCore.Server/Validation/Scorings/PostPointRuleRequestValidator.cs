using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PostPointRuleRequestValidator : AbstractValidator<PostPointRuleRequest>
    {
        public PostPointRuleRequestValidator(PostPointRuleModelValidator modelValidator)
        {
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
