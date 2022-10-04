using FluentValidation;
using iRLeagueApiCore.Common.Models;
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

namespace iRLeagueApiCore.Server.Handlers.Events
{
    public class EventHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public EventHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<EventEntity?> GetEventEntityAsync(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            return await dbContext.Events
                .Include(x => x.Sessions)
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .FirstOrDefaultAsync();
        }

        protected virtual async Task<EventEntity> MapToEventEntityAsync(LeagueUser user, PostEventModel postEvent, EventEntity target, CancellationToken cancellationToken)
        {
            target.Date = postEvent.Date;
            target.Duration = postEvent.Duration;
            target.EventType = postEvent.EventType;
            target.Name = postEvent.Name;
            MapToSessionEntityCollection(user, postEvent.Sessions, target.Sessions);
            target.Track = await GetTrackConfigEntityAsync(postEvent.TrackId, cancellationToken);
            return UpdateVersionEntity(user, target);
        }

        protected virtual async Task<EventEntity> MapToEventEntityAsync(LeagueUser user, PutEventModel putEvent, EventEntity target, CancellationToken cancellationToken)
        {
            return await MapToEventEntityAsync(user, (PostEventModel)putEvent, target, cancellationToken);
        }

        protected virtual void MapToSessionEntityCollection(LeagueUser user, IEnumerable<SessionModel> putSessions,
            ICollection<SessionEntity> target)
        {
            List<long> keepSubSessionIds = new List<long>();
            foreach (var putSession in putSessions)
            {
                // try to find subsession in target collection
                var sessionEntity = target.SingleOrDefault(x => x.SessionId == putSession.SessionId);
                // create new subsession if no previous id was given
                if (putSession.SessionId == 0)
                {
                    sessionEntity = new SessionEntity();
                    target.Add(sessionEntity);
                }
                if (sessionEntity == null)
                {
                    throw new InvalidOperationException($"Error while mapping SessionEntities to Event: SessionId:{putSession.SessionId} does not exist in target collection Sessions");
                }
                MapToSessionEntity(user, putSession, sessionEntity);
            }
            // remove subsessions that are not referenced
            var removeSessions = target
                .ExceptBy(putSessions.Select(x => x.SessionId), x => x.SessionId);
            foreach (var removeSession in removeSessions)
            {
                target.Remove(removeSession);
            }
        }

        protected virtual SessionEntity MapToSessionEntity(LeagueUser user, PutSessionModel putSession, SessionEntity target)
        {
            target.Name = putSession.Name;
            target.SessionType = putSession.SessionType;
            target.SessionNr = putSession.SessionNr;
            target.Laps = putSession.Laps;
            target.Duration = putSession.Duration;
            return UpdateVersionEntity(user, target);
        }

        protected virtual async Task<EventModel?> MapToEventModelAsync(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            return await dbContext.Events
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(MapToEventModelExpression)
                .FirstOrDefaultAsync();
        }

        protected virtual Expression<Func<EventEntity, EventModel>> MapToEventModelExpression => @event => new EventModel()
        {
            Date = @event.Date,
            Duration = @event.Duration,
            EventType = @event.EventType,
            Id = @event.EventId,
            LeagueId = @event.LeagueId,
            Name = @event.Name,
            ScheduleId = @event.ScheduleId,
            SeasonId = @event.Schedule.SeasonId,
            HasResult = @event.ScoredEventResults.Any(),
            Sessions = @event.Sessions.Select(session => new SessionModel()
            {
                HasResult = session.SessionResult != null,
                SessionNr = session.SessionNr,
                LeagueId = session.LeagueId,
                Name = session.Name,
                Laps = session.Laps,
                Duration = session.Duration,
                SessionId = session.SessionId,
                SessionType = session.SessionType,
                CreatedOn = session.CreatedOn,
                CreatedByUserId = session.CreatedByUserId,
                CreatedByUserName = session.CreatedByUserName,
                LastModifiedOn = session.LastModifiedOn,
                LastModifiedByUserId = session.LastModifiedByUserId,
                LastModifiedByUserName = session.LastModifiedByUserName
            }).ToList(),
            TrackId = @event.TrackId,
        };
    }
}
