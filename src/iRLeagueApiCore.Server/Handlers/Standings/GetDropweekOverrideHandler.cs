using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetDropweekOverrideRequest(long StandingId, long ScoredResultRowId) : IRequest<DropweekOverrideModel>;

public class GetDropweekOverrideHandler : StandingsHandlerBase<GetDropweekOverrideHandler, GetDropweekOverrideRequest, DropweekOverrideModel>
{
    public GetDropweekOverrideHandler(ILogger<GetDropweekOverrideHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDropweekOverrideRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<DropweekOverrideModel> Handle(GetDropweekOverrideRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var standingConfig = await dbContext.StandingConfigurations
            .FirstOrDefaultAsync(x => x.Standings.Any(y => y.StandingId == request.StandingId), cancellationToken)
            ?? throw new ResourceNotFoundException();
        var dropweek = await MapToDropweekOverrideModel(standingConfig.StandingConfigId, request.ScoredResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return dropweek;
    }
}
