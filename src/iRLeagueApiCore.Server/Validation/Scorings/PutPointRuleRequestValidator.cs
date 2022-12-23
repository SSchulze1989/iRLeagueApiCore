using iRLeagueApiCore.Server.Handlers.Scorings;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PutPointRuleRequestValidator : AbstractValidator<PutPointRuleRequest>
    {

        public PutPointRuleRequestValidator(PutPointRuleModelValidator modelValidator)
        {
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }
    }
}
