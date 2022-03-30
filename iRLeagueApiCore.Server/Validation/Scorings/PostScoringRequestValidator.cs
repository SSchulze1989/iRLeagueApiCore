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
    public class PostScoringRequestValidator : AbstractValidator<PostScoringRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PostScoringRequestValidator(LeagueDbContext dbContext, PostScoringModelValidator modelValidator)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("League id required")
                .MustAsync(LeagueExists)
                .WithMessage("League does not exist");
            RuleFor(x => x.SeasonId)
                .NotEmpty()
                .WithMessage("Season id required")
                .MustAsync(SeasonExists)
                .WithMessage("Season does not exist");
            RuleFor(x => x.Model)
                .SetValidator(modelValidator);
        }

        private async Task<bool> LeagueExists(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Leagues.AnyAsync(x => x.Id == leagueId);
        }

        private async Task<bool> SeasonExists(PostScoringRequest request, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.Seasons
                .Where(x => x.LeagueId == request.LeagueId)
                .AnyAsync(x => x.SeasonId == seasonId);
        }
    }
}
