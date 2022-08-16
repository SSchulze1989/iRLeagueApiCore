using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Sessions
{
    public class PostSessionToScheduleRequestValidator : AbstractValidator<PostSessionToScheduleRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PostSessionToScheduleRequestValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.ScheduleId)
                .MustAsync(ScheduleExists)
                .WithMessage("No entry for schedule found with combination of LeagueId and ScheduleId");
            RuleFor(x => x.Model.SubSessions)
                .NotNull()
                .WithMessage("SubSessions required");
            RuleForEach(x => x.Model.SubSessions)
                .Must(SubSessionIdValid)
                .WithMessage("Not allowed to target an existing subsession when posting a session -> SubSessionId must be \"0\"");
        }

        public async Task<bool> ScheduleExists(PostSessionToScheduleRequest request, long scheduleId, CancellationToken cancellationToken)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.ScheduleId == scheduleId)
                .AnyAsync(cancellationToken);
        }

        public bool SubSessionIdValid(PutSessionSubSessionModel subSession)
        {
            return subSession.SubSessionId == 0;
        }
    }
}
