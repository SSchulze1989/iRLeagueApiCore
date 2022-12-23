using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings;

public class PutPointRuleModelValidator : AbstractValidator<PutPointRuleModel>
{
    public PutPointRuleModelValidator(PostPointRuleModelValidator parentValidator)
    {
        Include(parentValidator);
    }
}
