using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Server.Validation.Reviews
{
    public class PutReviewRequestValidator : AbstractValidator<PutReviewRequest>
    {
        public PutReviewRequestValidator(LeagueDbContext dbContext)
        {
            RuleFor(x => x.Model)
                .SetValidator(new PostReviewModelValidator(dbContext));
        }
    }
}
