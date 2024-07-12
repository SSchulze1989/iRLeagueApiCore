using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetDropweekOverridesRequest(long StandingId) : IRequest<IEnumerable<DropweekOverrideModel>>;

public sealed class GetDropweekOverridesHandler : StandingsHandlerBase<GetDropweekOverridesHandler, GetDropweekOverridesRequest, IEnumerable<DropweekOverrideModel>>
{
    public GetDropweekOverridesHandler(ILogger<GetDropweekOverridesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDropweekOverridesRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<DropweekOverrideModel>> Handle(GetDropweekOverridesRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var dropweekOverrides = await MapToDropweekOverrideFromStandingAsync(request.StandingId, cancellationToken);
        return dropweekOverrides;
    }

    private async Task<IEnumerable<DropweekOverrideModel>> MapToDropweekOverrideFromStandingAsync(long standingId, CancellationToken cancellationToken)
    {
        if (await dbContext.Standings.AnyAsync(x => x.StandingId == standingId, cancellationToken) == false)
        {
            throw new ResourceNotFoundException();
        }

        var dropweekOverrides = await dbContext.DropweekOverrides
            .Where(x => x.ScoredResultRow.StandingRows.Any(y => y.StandingRow.StandingId == standingId))
            .Select(MapToDropweekOverrideExpression)
            .ToListAsync(cancellationToken);
        return dropweekOverrides;
    }
}