using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record GetEventRostersRequest(long EventId) : IRequest<IEnumerable<RosterModel>>;

public class GetEventRostersHandler : RostersHandlerBase<GetEventRostersHandler, GetEventRostersRequest, IEnumerable<RosterModel>>
{
    public GetEventRostersHandler(ILogger<GetEventRostersHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetEventRostersRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }
    
    public override async Task<IEnumerable<RosterModel>> Handle(GetEventRostersRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var rostersQuery = await GetRostersQuery(request.EventId, cancellationToken);
        var eventRosters = await rostersQuery
            .Select(MapToRosterModelExpression)
            .ToListAsync(cancellationToken);
        return eventRosters;
    }

    private async Task<IQueryable<RosterEntity>> GetRostersQuery(long eventId, CancellationToken cancellationToken)
    {
        var @event = await dbContext.Events
            .Include(x => x.ResultConfigs)
                .ThenInclude(x => x.ChampSeason)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
        var rosterIds = @event?.ResultConfigs
            .Select(x => x.ChampSeason.RosterId)
            .NotNull()
            .ToList() ?? [];
        return dbContext.Rosters
            .Where(x => rosterIds.Contains(x.RosterId))
            .AsNoTracking();
    }
}
