using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings;

public sealed class PostPointRuleModelValidator : AbstractValidator<PostPointRuleModel>
{
    public PostPointRuleModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleForEach(x => x.BonusPoints)
            .Must(KeyDoesNotContainDelimiter)
            .WithMessage("Keys are not allowed to contain ':' or ';'");
    }

    private bool KeyDoesNotContainDelimiter(KeyValuePair<string, int> pair)
    {
        return pair.Key.Contains(':') == false &&
            pair.Key.Contains(';') == false;
    }
}
