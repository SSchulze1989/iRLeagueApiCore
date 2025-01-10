using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetDropweekOverridesRequest(long StandingConfigId) : IRequest<IEnumerable<DropweekOverrideModel>>;

public sealed class GetDropweekOverridesHandler : StandingsHandlerBase<GetDropweekOverridesHandler, GetDropweekOverridesRequest, IEnumerable<DropweekOverrideModel>>
{
    public GetDropweekOverridesHandler(ILogger<GetDropweekOverridesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDropweekOverridesRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<DropweekOverrideModel>> Handle(GetDropweekOverridesRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var dropweekOverrides = await MapToDropweekOverrideFromStandingAsync(request.StandingConfigId, cancellationToken);
        return dropweekOverrides;
    }

    private async Task<IEnumerable<DropweekOverrideModel>> MapToDropweekOverrideFromStandingAsync(long standingConfigId, CancellationToken cancellationToken)
    {
        if (await dbContext.StandingConfigurations.AnyAsync(x => x.StandingConfigId == standingConfigId, cancellationToken) == false)
        {
            throw new ResourceNotFoundException();
        }

        var dropweekOverrides = await dbContext.DropweekOverrides
            .Where(x => x.StandingConfigId == standingConfigId)
            .Select(MapToDropweekOverrideModelExpression)
            .ToListAsync(cancellationToken);
        return dropweekOverrides;
    }
}