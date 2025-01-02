

using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record TriggerStandingsCalculationForEventRequest(long EventId) : IRequest<Unit>;

public class TriggerStandingsCalculationForEventHandler : StandingsHandlerBase<TriggerStandingsCalculationForEventHandler, TriggerStandingsCalculationForEventRequest, Unit>
{
    private readonly IStandingCalculationQueue calculationQueue;

    public TriggerStandingsCalculationForEventHandler(ILogger<TriggerStandingsCalculationForEventHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<TriggerStandingsCalculationForEventRequest>> validators, IStandingCalculationQueue calculationQueue) : base(logger, dbContext, validators)
    {
        this.calculationQueue = calculationQueue;
    }

    public override async Task<Unit> Handle(TriggerStandingsCalculationForEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var @event = await dbContext.Events
            .Where(x => x.EventId == request.EventId)
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new ResourceNotFoundException();
        calculationQueue.QueueStandingCalculationDebounced(@event.EventId, 0);
        return Unit.Value;
    }
}
