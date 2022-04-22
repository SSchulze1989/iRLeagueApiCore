using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public class SessionHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public SessionHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<SessionEntity> GetSessionEntityAsync(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SessionId == sessionId);
        }

        protected virtual async Task<SessionEntity> MapToSessionEntityAsync(long leagueId, LeagueUser user, PutSessionModel putSession, SessionEntity target,
            CancellationToken cancellationToken)
        {
            target.Date = putSession.Date;
            target.Duration = putSession.Duration;
            target.SubSessionNr = putSession.SubSessionNr;
            target.Laps = putSession.Laps;
            target.Name = putSession.Name;
            target.PracticeAttached = putSession.PracticeAttached;
            target.PracticeLength = putSession.PracticeLength;
            target.QualyAttached = putSession.QualyAttached;
            target.QualyLength = putSession.QualyLength;
            target.RaceLength = putSession.RaceLength;
            target.SessionTitle = putSession.SessionTitle;
            target.SessionType = putSession.SessionType;
            target.Track = await GetTrackConfigEntityAsync(putSession.TrackId, cancellationToken);
            return UpdateVersionEntity(user, target);
        }

        protected virtual async Task<GetSessionModel> MapToGetSessionModelAsync(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SessionId == sessionId)
                .Select(MapToGetSessionModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected virtual Expression<Func<SessionEntity, GetSessionModel>> MapToGetSessionModelExpression => x => new GetSessionModel()
        {
            SessionId = x.SessionId,
            ScheduleId = x.ScheduleId,
            LeagueId = x.LeagueId,
            PracticeAttached = x.PracticeAttached ?? false,
            QualyAttached = x.QualyAttached ?? false,
            PracticeLength = x.PracticeLength,
            QualyLength = x.QualyLength,
            Date = x.Date,
            Duration = x.Duration,
            Laps = x.Laps ?? 0,
            RaceLength = x.RaceLength,
            Name = x.Name,
            SessionTitle = x.SessionTitle,
            SessionType = x.SessionType,
            SubSessionIds = x.SubSessions.Select(x => x.SessionId),
            ParentSessionId = x.ParentSessionId,
            SubSessionNr = x.SubSessionNr,
            TrackId = x.TrackId,
            HasResult = x.Result != null,
            CreatedOn = x.CreatedOn,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUserName = x.CreatedByUserName,
            LastModifiedOn = x.LastModifiedOn,
            LastModifiedByUserId = x.LastModifiedByUserId,
            LastModifiedByUserName = x.LastModifiedByUserName
        };
    }
}
