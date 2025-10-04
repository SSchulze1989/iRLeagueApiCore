

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record DeleteTriggerRequest(long triggerId) : IRequest<Unit>;

public class DeleteTriggerHandler : TriggersHandlerBase<DeleteTriggerHandler, DeleteTriggerRequest, Unit>
{
    public DeleteTriggerHandler(ILogger<DeleteTriggerHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteTriggerRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteTriggerRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteTrigger = await dbContext.Triggers
            .Where(x => x.TriggerId == request.triggerId)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new ResourceNotFoundException();
        dbContext.Triggers.Remove(deleteTrigger);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
