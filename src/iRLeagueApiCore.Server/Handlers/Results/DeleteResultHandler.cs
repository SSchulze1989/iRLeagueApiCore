namespace iRLeagueApiCore.Server.Handlers.Results;

public record DeleteResultRequest(long EventId) : IRequest<Unit>;

public sealed class DeleteResultHandler : ResultHandlerBase<DeleteResultHandler, DeleteResultRequest, Unit>
{
    private readonly IPublisher publisher;

    public DeleteResultHandler(ILogger<DeleteResultHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<DeleteResultRequest>> validators, IPublisher publisher) :
        base(logger, dbContext, validators)
    {
        this.publisher = publisher;
    }

    public override async Task<Unit> Handle(DeleteResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var @event = await dbContext.Events
            .Where(x => x.EventId == request.EventId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var deleteEventResults = await GetScoredEventResults(@event.EventId, cancellationToken);
        var deleteStandings = await GetEventStandings(@event.EventId, cancellationToken);
        foreach (var result in deleteEventResults)
        {
            await SafeDeleteScoredEventResult(result);
        }
        foreach (var standing in deleteStandings)
        {
            dbContext.Standings.Remove(standing);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new ResultDeletedEventNotification(@event.EventId), cancellationToken);
        return Unit.Value;
    }

    private async Task<IEnumerable<ScoredEventResultEntity>> GetScoredEventResults(long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.ScoredEventResults
            .Where(x => x.EventId == eventId)
            .Include(x => x.ScoredSessionResults)
                .ThenInclude(x => x.ScoredResultRows)
                    .ThenInclude(x => x.AddPenalties)
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<StandingEntity>> GetEventStandings(long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Standings
            .Where(x => x.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    private async Task SafeDeleteScoredEventResult(ScoredEventResultEntity eventResult)
    {
        foreach (var sessionResult in eventResult.ScoredSessionResults)
        {
            await SafeDeleteScoredSessionResult(sessionResult);
        }
        dbContext.Remove(eventResult);
    }

    private async Task SafeDeleteScoredSessionResult(ScoredSessionResultEntity sessionResult)
    {
        // Delete penalties
        var penalties = sessionResult.ScoredResultRows
            .SelectMany(x => x.AddPenalties)
            .ToList();
        dbContext.RemoveRange(penalties);
        dbContext.Remove(sessionResult);
    }
}
