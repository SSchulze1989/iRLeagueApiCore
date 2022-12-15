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

namespace iRLeagueApiCore.Server.Handlers.Events
{
    public record PostEventToScheduleRequest(long LeagueId, long ScheduleId, LeagueUser User, PostEventModel Event) : IRequest<EventModel>;

    public class PostEventToScheduleHandler : EventHandlerBase<PostEventToScheduleHandler, PostEventToScheduleRequest>, IRequestHandler<PostEventToScheduleRequest, EventModel>
    {
        public PostEventToScheduleHandler(ILogger<PostEventToScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostEventToScheduleRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<EventModel> Handle(PostEventToScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postEvent = await CreateEventOnScheduleAsync(request.User, request.LeagueId, request.ScheduleId, cancellationToken);
            postEvent = await MapToEventEntityAsync(request.User, request.Event, postEvent, cancellationToken);
            dbContext.Events.Add(postEvent);
            await dbContext.SaveChangesAsync();
            var getEvent = await MapToEventModelAsync(request.LeagueId, postEvent.EventId, cancellationToken: cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getEvent;
        }

        protected virtual async Task<EventEntity> CreateEventOnScheduleAsync(LeagueUser user, long leagueId, long scheduleId, CancellationToken cancellationToken)
        {
            var schedule = await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            var @event = CreateVersionEntity(user, new EventEntity());
            schedule.Events.Add(@event);
            return @event;
        }
    }
}
