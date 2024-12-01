using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record TriggerResultCalculationCommand(long EventId) : IRequest;

public sealed class TriggerResultCalculationHandler : ResultHandlerBase<TriggerResultCalculationHandler,  TriggerResultCalculationCommand, Unit>
{
    private readonly IResultCalculationQueue calculationQueue;

    public TriggerResultCalculationHandler(ILogger<TriggerResultCalculationHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<TriggerResultCalculationCommand>> validators, IResultCalculationQueue calculationQueue) : base(logger, dbContext, validators)
    {
        this.calculationQueue = calculationQueue;
    }

    public override async Task<Unit> Handle(TriggerResultCalculationCommand request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var @event = await dbContext.Events
            .Where(x => x.EventId == request.EventId)
            .FirstOrDefaultAsync(cancellationToken);
        if (@event is null)
        {
            return Unit.Value;
        }

        calculationQueue.QueueEventResultDebounced(@event.EventId, 0);
        return Unit.Value;
    }
}
