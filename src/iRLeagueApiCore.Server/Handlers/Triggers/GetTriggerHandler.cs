using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record GetTriggerRequest(long triggerId) : IRequest<TriggerModel>;

public class GetTriggerHandler : TriggersHandlerBase<GetTriggerHandler, GetTriggerRequest, TriggerModel>
{
    public GetTriggerHandler(ILogger<GetTriggerHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetTriggerRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public override async Task<TriggerModel> Handle(GetTriggerRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getTrigger = await dbContext.Triggers
            .Where(x => x.TriggerId == request.triggerId)
            .Select(MapToTriggerExpression)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getTrigger;
    }
}
