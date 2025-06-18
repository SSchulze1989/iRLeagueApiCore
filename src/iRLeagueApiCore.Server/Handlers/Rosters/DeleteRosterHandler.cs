

using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record DeleteRosterRequest(long RosterId, LeagueUser User) : IRequest<Unit>;

public class DeleteRosterHandler : RostersHandlerBase<DeleteRosterHandler, DeleteRosterRequest, Unit>
{
    public DeleteRosterHandler(ILogger<DeleteRosterHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteRosterRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteRosterRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteRoster = await GetRosterEntity(request.RosterId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        DeleteRoster(deleteRoster, request.User);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void DeleteRoster(RosterEntity entity, LeagueUser user)
    {
        entity.IsArchived = true;
        UpdateVersionEntity(user, entity);
    }
}
