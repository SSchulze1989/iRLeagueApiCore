using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record PostRosterRequest(PostRosterModel Model, LeagueUser User) : IRequest<RosterModel>;

public class PostRosterHandler : RostersHandlerBase<PostRosterHandler, PostRosterRequest, RosterModel>
{
    public PostRosterHandler(ILogger<PostRosterHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostRosterRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RosterModel> Handle(PostRosterRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postRoster = CreateVersionEntity<RosterEntity>(request.User, new());
        postRoster = MapToRosterEntity(postRoster, request.Model, request.User);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getRoster = await GetRosterModel(postRoster.RosterId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getRoster;
    }
}
