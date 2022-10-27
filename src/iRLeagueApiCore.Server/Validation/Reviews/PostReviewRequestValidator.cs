using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Reviews
{
    public class PostReviewRequestValidator : AbstractValidator<PostReviewToSessionRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PostReviewRequestValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.SessionId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("No session selected")
                .MustAsync((request, sessionId, cancellationToken) => SessionisValid(request.LeagueId, sessionId, cancellationToken))
                .WithMessage("Selected session does not exist");
            RuleFor(x => x.Model)
                .SetValidator(new PostReviewModelValidator(dbContext));
        }

        private async Task<bool> SessionisValid(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SessionId == sessionId)
                .AnyAsync(cancellationToken);
        }
    }
}
