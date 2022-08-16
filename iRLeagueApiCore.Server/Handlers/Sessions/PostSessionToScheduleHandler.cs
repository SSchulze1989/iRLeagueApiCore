using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record PostSessionToScheduleRequest(long LeagueId, long ScheduleId, LeagueUser User, PostSessionModel Model) : IRequest<SessionModel>;

    public class PostSessionToScheduleHandler : SessionHandlerBase<PostSessionToScheduleHandler, PostSessionToScheduleRequest>, IRequestHandler<PostSessionToScheduleRequest, SessionModel>
    {
        public PostSessionToScheduleHandler(ILogger<PostSessionToScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostSessionToScheduleRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<SessionModel> Handle(PostSessionToScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postSession = await CreateSessionOnScheduleAsync(request.LeagueId, request.ScheduleId, request.User, cancellationToken);
            postSession = await MapToSessionEntityAsync(request.User, request.Model, postSession, cancellationToken);
            dbContext.Sessions.Add(postSession);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSession = await MapToGetSessionModelAsync(request.LeagueId, postSession.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSession;
        }

        protected async virtual Task<SessionEntity> CreateSessionOnScheduleAsync(long leagueId, long scheduleId, LeagueUser user, CancellationToken cancellationToken)
        {
            var schedule = await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Include(x => x.Sessions)
                .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            var session = CreateVersionEntity(user, new SessionEntity());
            schedule.Sessions.Add(session);
            return session;
        }
    }
}
