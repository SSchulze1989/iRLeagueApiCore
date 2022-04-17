using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PutScoringModelValidator : AbstractValidator<PutScoringModel>
    {
        public PutScoringModelValidator(PostScoringModelValidator postScoringModelValidator)
        {
            Include(postScoringModelValidator);
        }
    }
}
