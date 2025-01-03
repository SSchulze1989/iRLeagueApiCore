

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record DeleteDropweekOverrideRequest(long StandingId, long ScoredResultRowId) : IRequest<Unit>;

public class DeleteDropweekOverrideHandler : StandingsHandlerBase<DeleteDropweekOverrideHandler, DeleteDropweekOverrideRequest, Unit>
{
    public DeleteDropweekOverrideHandler(ILogger<DeleteDropweekOverrideHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteDropweekOverrideRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteDropweekOverrideRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var standingConfig = await dbContext.StandingConfigurations
            .FirstOrDefaultAsync(x => x.Standings.Any(y => y.StandingId == request.StandingId), cancellationToken)
            ?? throw new ResourceNotFoundException();
        var dropweek = await GetDropweekOverrideEntityAsync(standingConfig.StandingConfigId, request.ScoredResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.DropweekOverrides.Remove(dropweek);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<DropweekOverrideEntity?> GetDropweekOverrideEntityAsync(long standingConfigId, long scoredResultRowId, CancellationToken cancellationToken)
    {
        return await dbContext.DropweekOverrides
            .Where(x => x.StandingConfigId == standingConfigId)
            .Where(x => x.ScoredResultRowId == scoredResultRowId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
