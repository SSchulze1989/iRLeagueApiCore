using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public class PostScoringModelValidator : AbstractValidator<PostScoringModel>
    {
        private readonly LeagueDbContext dbContext;

        public PostScoringModelValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.BasePoints)
                .NotNull();
            RuleFor(x => x.BonusPoints)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Cannot be null")
                .Must(BonusPointsValid)
                .WithMessage("Collection contains invalid values");
            RuleFor(x => x.ExtScoringSourceId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage($"Cannot be null when {nameof(PostScoringModel.TakeResultsFromExtSource)} is true")
                .MustAsync(ScoringExists)
                .WithMessage("Scoring does not exist")
                .When(x => x.TakeResultsFromExtSource == true);
        }

        private bool BonusPointsValid(IEnumerable<string> bonusPoints)
        {
            var result = true;
            foreach (var point in bonusPoints)
            {
                // evaluate regular expression for each entry
                result &= Regex.Match(point, @"[pqPQ].*[0-9.].*:.*[0-9.].*").Success;
            }
            return result;
        }

        private async Task<bool> ScoringExists(long? scoringId, CancellationToken cancellationToken)
        {
            return await dbContext.Scorings
                .AnyAsync(x => x.ScoringId == scoringId);
        }
    }
}
