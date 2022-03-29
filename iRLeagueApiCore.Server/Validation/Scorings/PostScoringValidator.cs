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
    public class PostScoringValidator : AbstractValidator<PostScoringRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PostScoringValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("Invalid league id")
                .MustAsync(LeagueExists)
                .WithMessage("League does not exist");
            RuleFor(x => x.BasePoints)
                .NotNull()
                .WithMessage("Cannot be null");
            RuleFor(x => x.BonusPoints)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Cannot be null")
                .Must(BonusPointsValid);
            RuleFor(x => x.ExtScoringSourceId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage($"Cannot be null when {nameof(PostScoringRequest.TakeResultsFromExtSource)} is true")
                .MustAsync(ScoringExists)
                .WithMessage("Scoring does not exist")
                .When(x => x.TakeResultsFromExtSource == true);
        }

        private async Task<bool> LeagueExists(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Leagues.AnyAsync(x => x.Id == leagueId);
        }

        private bool BonusPointsValid(IEnumerable<string> bonusPoints)
        {
            var result = true;
            foreach (var point in bonusPoints)
            {
                // evaluate regular expression for each entry
                result &= Regex.Match(point, @"[pqPQ].*[0-9].*:.*[0-9].*").Success;
            }
            return result;
        }

        private async Task<bool> ScoringExists(PostScoringRequest request, long? scoringId, CancellationToken cancellationToken)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == request.LeagueId)
                .AnyAsync(x => x.ScoringId == scoringId);
        }
    }
}
