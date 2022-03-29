using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PutScoringValidator : AbstractValidator<PutScoringRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PutScoringValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            Include(new PostScoringValidator(dbContext));
        }
    }
}
