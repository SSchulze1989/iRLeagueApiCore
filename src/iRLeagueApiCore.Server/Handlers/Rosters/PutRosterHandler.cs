using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record PutRosterRequest(long rosterId, PutRosterModel Model, LeagueUser User) : IRequest<RosterModel>;

public class PutRosterHandler : RostersHandlerBase<PutRosterHandler, PutRosterRequest, RosterModel>
{
    public PutRosterHandler(ILogger<PutRosterHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutRosterRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RosterModel> Handle(PutRosterRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putRoster = await GetRosterEntity(request.rosterId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        MapToRosterEntity(putRoster, request.Model, request.User);
        putRoster.IsArchived = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        var getRoster = await GetRosterModel(request.rosterId, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource was not found");
        return getRoster;
    }
}
