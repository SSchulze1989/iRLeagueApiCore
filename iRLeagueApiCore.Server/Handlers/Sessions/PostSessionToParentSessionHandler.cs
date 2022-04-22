using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public record PostSessionToParentSessionRequest(long LeagueId, long ParentSessionId, LeagueUser User, PostSessionModel Model) : IRequest<GetSessionModel>;

    public class PostSessionToParentSessionHandler : SessionHandlerBase<PostSessionToParentSessionHandler, PostSessionToParentSessionRequest>, IRequestHandler<PostSessionToParentSessionRequest, GetSessionModel>
    {
        public PostSessionToParentSessionHandler(ILogger<PostSessionToParentSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostSessionToParentSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetSessionModel> Handle(PostSessionToParentSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postSession = await CreateSessionOnParentAsync(request.LeagueId, request.ParentSessionId, request.User, cancellationToken);
            postSession = await MapToSessionEntityAsync(request.LeagueId, request.User, request.Model, postSession, cancellationToken);
            dbContext.Sessions.Add(postSession);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSession = await MapToGetSessionModelAsync(request.LeagueId, postSession.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSession;
        }

        protected async virtual Task<SessionEntity> CreateSessionOnParentAsync(long leagueId, long sessionId, LeagueUser user, CancellationToken cancellationToken)
        {
            var parent = await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Include(x => x.SubSessions)
                .SingleOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            var session = CreateVersionEntity(user, new SessionEntity());
            parent.SubSessions.Add(session);
            return session;
        }
    }
}
