using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record PostTriggerRequest(LeagueUser User, PostTriggerModel Trigger) : IRequest<TriggerModel>;

public class PostTriggerHandler : TriggersHandlerBase<PostTriggerHandler, PostTriggerRequest, TriggerModel>
{
    public PostTriggerHandler(ILogger<PostTriggerHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostTriggerRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<TriggerModel> Handle(PostTriggerRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await dbContext.Leagues
            .Include(x => x.Triggers)
            .FirstOrDefaultAsync(x => x.Id == dbContext.LeagueProvider.LeagueId, cancellationToken)
            ?? throw new InvalidOperationException("Current league not found");
        var newTrigger = CreateVersionEntity<TriggerEntity>(request.User, new());
        newTrigger = MapToTriggerEntity(newTrigger, request.Trigger);
        league.Triggers.Add(newTrigger);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getTrigger = await dbContext.Triggers
            .Where(x => x.TriggerId == newTrigger.TriggerId)
            .Select(MapToTriggerExpression)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getTrigger;
    }
}
