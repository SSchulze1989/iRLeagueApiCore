using FluentValidation;
using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Server.Validation.Seasons
{
    public class PostSeasonModelValidator : AbstractValidator<PostSeasonModel>
    {
        public PostSeasonModelValidator(PutSeasonModelValidator putSeasonValidator)
        {
            Include(putSeasonValidator);
        }
    }
}
