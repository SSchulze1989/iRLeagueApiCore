using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record GetLeagueTriggersRequest() : IRequest<IEnumerable<TriggerModel>>;

public class GetLeagueTriggersHandler : TriggersHandlerBase<GetLeagueTriggersHandler, GetLeagueTriggersRequest, IEnumerable<TriggerModel>>
{
    public GetLeagueTriggersHandler(ILogger<GetLeagueTriggersHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeagueTriggersRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<TriggerModel>> Handle(GetLeagueTriggersRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var triggers = await dbContext.Triggers
            .Select(MapToTriggerExpression)
            .ToListAsync(cancellationToken);
        return triggers;
    }
}
