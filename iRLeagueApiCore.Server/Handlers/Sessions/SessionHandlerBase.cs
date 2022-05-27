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

        protected virtual async Task<SessionEntity> MapToSessionEntityAsync(LeagueUser user, PutSessionModel putSession, SessionEntity target,
            CancellationToken cancellationToken)
        {
            target.Date = putSession.Date;
            target.Duration = putSession.Duration;
            target.Name = putSession.Name;
            target.SessionType = putSession.SessionType;
            target.Track = await GetTrackConfigEntityAsync(putSession.TrackId, cancellationToken);
            MapToSubSessionEntityCollection(user, putSession.SubSessions, target.SubSessions, cancellationToken);
            return UpdateVersionEntity(user, target);
        }

        protected virtual void MapToSubSessionEntityCollection(LeagueUser user, IEnumerable<PutSessionSubSessionModel> putSubSessions,
            ICollection<SubSessionEntity> target, CancellationToken cancellationToken)
        {
            List<long> keepSubSessionIds = new List<long>();
            foreach(var putSubSession in putSubSessions)
            {
                // try to find subsession in target collection
                var subSessionEntity = target.SingleOrDefault(x => x.SubSessionId == putSubSession.SubSessionId);
                // create new subsession if no previous id was given
                if (putSubSession.SubSessionId == 0)
                {
                    subSessionEntity = new SubSessionEntity();
                    target.Add(subSessionEntity);
                }
                if (subSessionEntity == null)
                {
                    throw new InvalidOperationException($"Error while mapping SubSessionEntities to Session: SubSessionId:{putSubSession.SubSessionId} does not exist in target collection SubSessions");
                }
                MapToSubSessionEntity(user, putSubSession, subSessionEntity);
            }
            // remove subsessions that are not referenced
            var removeSubSessions = target
                .ExceptBy(putSubSessions.Select(x => x.SubSessionId), x => x.SubSessionId);
            foreach (var removeSubSession in removeSubSessions)
            {
                target.Remove(removeSubSession);
            }
        }

        protected virtual SubSessionEntity MapToSubSessionEntity(LeagueUser user, PutSessionSubSessionModel putSubSession, SubSessionEntity target)
        {
            target.Duration = putSubSession.Duration;
            target.StartOffset = putSubSession.StartOffset;
            target.Laps = putSubSession.Laps;
            target.Name = putSubSession.Name;
            target.SessionType = putSubSession.SessionType;
            target.SubSessionNr = putSubSession.SubSessionNr;
            return UpdateVersionEntity(user, target);
        }

        protected virtual async Task<SessionModel> MapToGetSessionModelAsync(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SessionId == sessionId)
                .Select(MapToGetSessionModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected virtual Expression<Func<SessionEntity, SessionModel>> MapToGetSessionModelExpression => x => new SessionModel()
        {
            SessionId = x.SessionId,
            ScheduleId = x.ScheduleId,
            LeagueId = x.LeagueId,
            Date = x.Date,
            Duration = x.Duration,
            Name = x.Name,
            SessionType = x.SessionType,
            SubSessions = x.SubSessions
                .AsQueryable()
                .OrderBy(x => x.SubSessionNr)
                .Select(MapToGetSubSessionModelExpression)
                .ToList(),
            TrackId = x.TrackId,
            HasResult = x.Result != null,
            CreatedOn = x.CreatedOn,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUserName = x.CreatedByUserName,
            LastModifiedOn = x.LastModifiedOn,
            LastModifiedByUserId = x.LastModifiedByUserId,
            LastModifiedByUserName = x.LastModifiedByUserName
        };

        protected virtual Expression<Func<SubSessionEntity, SubSessionModel>> MapToGetSubSessionModelExpression => x => new SubSessionModel()
        {
            SubSessionId = x.SubSessionId,
            SubSessionNr = x.SubSessionNr,
            Name = x.Name,
            SessionType = x.SessionType,
            StartOffset = x.StartOffset,
            Duration = x.Duration,
            Laps = x.Laps,
        };
    }
}
