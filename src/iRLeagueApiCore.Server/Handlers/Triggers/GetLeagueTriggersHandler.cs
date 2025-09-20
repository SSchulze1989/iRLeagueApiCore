using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record GetTriggersRequest(long LeagueId) : IRequest<IEnumerable<TriggerModel>>;

public class GetLeagueTriggersHandler : TriggersHandlerBase<GetLeagueTriggersHandler, GetTriggersRequest, IEnumerable<TriggerModel>>
{
    public GetLeagueTriggersHandler(ILogger<GetLeagueTriggersHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetTriggersRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<TriggerModel>> Handle(GetTriggersRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var triggers = await dbContext.Triggers
            .Select(MapToTriggerExpression)
            .ToListAsync(cancellationToken);
        return triggers;
    }
}
