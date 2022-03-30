using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PutScoringModelValidator : PostScoringModelValidator
    {
        public PutScoringModelValidator(LeagueDbContext dbContext) : base(dbContext)
        {
        }
    }
}
