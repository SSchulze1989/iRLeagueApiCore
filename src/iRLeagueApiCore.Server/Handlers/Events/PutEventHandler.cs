using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Events
{
    public record PutEventRequest(long LeagueId, long EventId, LeagueUser User, PutEventModel Event) : IRequest<EventModel>;

    public class PutEventHandler : EventHandlerBase<PutEventHandler, PutEventRequest>, IRequestHandler<PutEventRequest, EventModel>
    {
        public PutEventHandler(ILogger<PutEventHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutEventRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<EventModel> Handle(PutEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putEvent = await GetEventEntityAsync(request.LeagueId, request.EventId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putEvent = await MapToEventEntityAsync(request.User, request.Event, putEvent, cancellationToken);
            dbContext.SaveChanges();
            var getEvent = await MapToEventModelAsync(request.LeagueId, putEvent.EventId, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Created resource was not found");
            return getEvent;
        }
    }
}
