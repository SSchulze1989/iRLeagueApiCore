

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record DeleteRosterEntryRequest(long RosterId, long MemberId) : IRequest<Unit>;

public class DeleteRosterEntryHandler : RostersHandlerBase<DeleteRosterEntryHandler, DeleteRosterEntryRequest, Unit>
{
    public DeleteRosterEntryHandler(ILogger<DeleteRosterEntryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteRosterEntryRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteRosterEntryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var roster = await GetRosterEntity(request.RosterId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        var entry = roster.RosterEntries.FirstOrDefault(e => e.MemberId == request.MemberId)
            ?? throw new ResourceNotFoundException($"Roster entry for member {request.MemberId} not found in roster {request.RosterId}.");
        roster.RosterEntries.Remove(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
