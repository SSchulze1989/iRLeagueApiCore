using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record PutTriggerRequest(long triggerId, LeagueUser User, PutTriggerModel Model) : IRequest<TriggerModel>;

public class PutTriggerHandler : TriggersHandlerBase<PutTriggerHandler, PutTriggerRequest, TriggerModel>
{
    public PutTriggerHandler(ILogger<PutTriggerHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutTriggerRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<TriggerModel> Handle(PutTriggerRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putTrigger = await dbContext.Triggers
            .Where(x => x.TriggerId == request.triggerId)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new ResourceNotFoundException();
        putTrigger = MapToTriggerEntity(putTrigger, request.Model);
        putTrigger = UpdateVersionEntity(request.User, putTrigger);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getTrigger = await dbContext.Triggers
            .Where(x => x.TriggerId == putTrigger.TriggerId)
            .Select(MapToTriggerExpression)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Updated resource was not found");
        return getTrigger;
    }
}
