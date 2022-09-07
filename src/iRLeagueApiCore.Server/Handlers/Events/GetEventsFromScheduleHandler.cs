using FluentValidation;
using iRLeagueApiCore.Common.Models;
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
    public record GetEventsFromScheduleRequest(long LeagueId, long ScheduleId) : IRequest<IEnumerable<EventModel>>;

    public class GetEventsFromScheduleHandler : EventHandlerBase<GetEventsFromScheduleHandler, GetEventsFromScheduleRequest>,
        IRequestHandler<GetEventsFromScheduleRequest, IEnumerable<EventModel>>
    {
        public GetEventsFromScheduleHandler(ILogger<GetEventsFromScheduleHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetEventsFromScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<EventModel>> Handle(GetEventsFromScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getEvents = await MapToGetEventsFromScheduleAsync(request.LeagueId, request.ScheduleId, cancellationToken);
            return getEvents;
        }

        protected virtual async Task<IEnumerable<EventModel>> MapToGetEventsFromScheduleAsync(long leagueId, long scheduleId, CancellationToken cancellationToken)
        {
            return await dbContext.Events
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ScheduleId == scheduleId)
                .Select(MapToEventModelExpression)
                .ToListAsync();
        }
    }
}
