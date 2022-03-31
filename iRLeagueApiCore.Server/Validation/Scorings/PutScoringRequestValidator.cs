using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Scorings
{
    public class PutScoringRequestValidator : AbstractValidator<PutScoringRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PutScoringRequestValidator(LeagueDbContext dbContext, PutScoringModelValidator modelValidator)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("League id required")
                .MustAsync(LeagueExists)
                .WithMessage("League does not exist");
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }

        private async Task<bool> LeagueExists(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Leagues.AnyAsync(x => x.Id == leagueId);
        }
    }
}
