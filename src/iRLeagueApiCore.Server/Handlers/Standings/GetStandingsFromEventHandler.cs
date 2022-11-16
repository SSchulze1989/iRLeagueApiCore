using iRLeagueApiCore.Common.Models.Standings;

namespace iRLeagueApiCore.Server.Handlers.Standings
{
    public record GetStandingsFromEventRequest(long LeagueId, long EventId) : IRequest<IEnumerable<StandingsModel>>;

    public sealed class GetStandingsFromEventHandler : StandingsHandlerBase<GetStandingsFromEventHandler, GetStandingsFromEventRequest>,
        IRequestHandler<GetStandingsFromEventRequest, IEnumerable<StandingsModel>>
    {
        public GetStandingsFromEventHandler(ILogger<GetStandingsFromEventHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetStandingsFromEventRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<StandingsModel>> Handle(GetStandingsFromEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getStandings = await MapToStandingModelFromEventAsync(request.LeagueId, request.EventId, cancellationToken);
            return getStandings;
        }

        private async Task<IEnumerable<StandingsModel>> MapToStandingModelFromEventAsync(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            return await dbContext.Standings
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(MapToStandingModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
