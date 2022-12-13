using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Seasons;

public class PutSeasonModelValidator : AbstractValidator<PutSeasonModel>
{
    public PutSeasonModelValidator(PostSeasonModelValidator putSeasonValidator)
    {
        Include(putSeasonValidator);
    }
}
