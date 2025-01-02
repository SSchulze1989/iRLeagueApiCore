using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record PutDropweekOverrideRequest(long StandingId, long ScoredResultRowId, PutDropweekOverrideModel Model) : IRequest<DropweekOverrideModel>;

public class PutDropweekOverrideHandler : StandingsHandlerBase<PutDropweekOverrideHandler, PutDropweekOverrideRequest, DropweekOverrideModel>
{
    public PutDropweekOverrideHandler(ILogger<PutDropweekOverrideHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutDropweekOverrideRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<DropweekOverrideModel> Handle(PutDropweekOverrideRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putDropweek = await GetOrCreateDropweekOverrideEntityAsync(request.StandingId, request.ScoredResultRowId, cancellationToken);
        putDropweek = MapToDropweekOverrideEntity(putDropweek, request.Model);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getDropweek = await MapToDropweekOverrideModel(putDropweek.StandingConfigId, putDropweek.ScoredResultRowId, cancellationToken)
            ?? throw new InvalidOperationException("Created or updated Resource not found");
        return getDropweek;
    }

    private async Task<DropweekOverrideEntity> GetOrCreateDropweekOverrideEntityAsync(long standingId, long scoredResultRowId, CancellationToken cancellationToken)
    {
        var standingConfig = await dbContext.StandingConfigurations
            .FirstOrDefaultAsync(x => x.Standings.Any(y => y.StandingId == standingId), cancellationToken)
            ?? throw new ResourceNotFoundException();
        var dropweek = await dbContext.DropweekOverrides
            .Where(x => x.StandingConfigId == standingConfig.StandingConfigId)
            .Where(x => x.ScoredResultRowId == scoredResultRowId)
            .FirstOrDefaultAsync(cancellationToken);

        if (dropweek is not null)
        {
            return dropweek;
        }

        var scoredResultRow = await dbContext.ScoredResultRows
            .FirstOrDefaultAsync(x => x.ScoredResultRowId == scoredResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dropweek = new()
        {
            StandingConfig = standingConfig,
            ScoredResultRow = scoredResultRow,
        };
        dbContext.DropweekOverrides.Add(dropweek);
        return dropweek;
    }

    private static DropweekOverrideEntity MapToDropweekOverrideEntity(DropweekOverrideEntity entity, PutDropweekOverrideModel model)
    {
        entity.ShouldDrop = model.ShouldDrop;
        return entity;
    }
}
