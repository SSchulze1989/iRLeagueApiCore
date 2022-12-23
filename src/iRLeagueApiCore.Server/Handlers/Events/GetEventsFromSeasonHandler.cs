using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Events;

public record GetEventsFromSeasonRequest(long LeagueId, long SeasonId, bool IncludeDetails) : IRequest<IEnumerable<EventModel>>;

public class GetEventsFromSeasonHandler : EventHandlerBase<GetEventsFromSeasonHandler, GetEventsFromSeasonRequest>,
    IRequestHandler<GetEventsFromSeasonRequest, IEnumerable<EventModel>>
{
    public GetEventsFromSeasonHandler(ILogger<GetEventsFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetEventsFromSeasonRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<EventModel>> Handle(GetEventsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getEvents = await MapToEventsFromSeasonAsync(request.LeagueId, request.SeasonId, request.IncludeDetails, cancellationToken);
        return getEvents;
    }

    protected virtual async Task<IEnumerable<EventModel>> MapToEventsFromSeasonAsync(long leagueId, long seasonId, bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.Schedule.SeasonId == seasonId)
            .Select(MapToEventModelExpression(includeDetails))
            .ToListAsync();
    }
}
